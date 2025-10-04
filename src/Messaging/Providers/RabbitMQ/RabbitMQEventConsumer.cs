using System.Text;
using System.Text.Json;

using Bsfranca2.Core;
using Bsfranca2.Messaging.Configurations;
using Bsfranca2.Messaging.Contracts;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

internal sealed class RabbitMQEventConsumer(
    ILogger<RabbitMQEventConsumer> logger,
    MessagingOptions options,
    RabbitMQConnectionFactory connectionFactory,
    IEventRoutingRegistry eventRoutingRegistry,
    IMessageProcessor messageProcessor,
    IHostEnvironment environment,
    IHandlerRegistry handlerRegistry,
    IQueueEventHandlerResolver queueEventResolver
) : IEventConsumer, IDisposable, IAsyncDisposable
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true
    };

    private readonly string _consumerNamePrefix = environment.ApplicationName;
    private readonly List<string> _consumerTags = new();
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed;

    private async Task EnsureConnectionAsync()
    {
        if (_connection is not { IsOpen: true } || _channel is not { IsOpen: true })
        {
            _connection = await connectionFactory.GetConsumerConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            RabbitMQEventConsumerLogs.LogStart(logger);

            await EnsureConnectionAsync();

            int startedConsumers = 0;
            foreach (QueueConfiguration queue in options.Queues)
            {
                Type[] eventTypes = queueEventResolver.GetEventTypesForQueue(queue.Name).ToArray();
                if (eventTypes.Length != 0 && eventTypes.Any(handlerRegistry.HasHandlerFor))
                {
                    await StartConsumerForQueue(queue, cancellationToken);
                    startedConsumers++;
                }
            }

            RabbitMQEventConsumerLogs.LogStartComplete(logger, startedConsumers);
        }
        catch (Exception ex)
        {
            RabbitMQEventConsumerLogs.LogStartFailed(logger, ex);
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            RabbitMQEventConsumerLogs.LogStop(logger);

            foreach (string consumerTag in _consumerTags)
            {
                try
                {
                    await _channel!.BasicCancelAsync(consumerTag, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    RabbitMQEventConsumerLogs.LogCancelConsumerError(logger, ex, consumerTag);
                }
            }

            _consumerTags.Clear();

            RabbitMQEventConsumerLogs.LogStopComplete(logger);
        }
        catch (Exception ex)
        {
            RabbitMQEventConsumerLogs.LogStopFailed(logger, ex);
        }
    }

    private async Task StartConsumerForQueue(QueueConfiguration queueConfig, CancellationToken cancellationToken)
    {
        try
        {
            if (queueConfig.PrefetchCount != null)
            {
                await _channel!.BasicQosAsync(0, (ushort)queueConfig.PrefetchCount.Value, false, cancellationToken);
            }

            AsyncEventingBasicConsumer consumer = new(_channel!);
            consumer.ReceivedAsync += async (_, ea) => await ProcessMessage(ea, queueConfig);

            string consumerTag = $"{_consumerNamePrefix}-{queueConfig.Name}-{Guid.CreateVersion7():N}";

            string finalTag = await _channel!.BasicConsumeAsync(
                queueConfig.Name,
                false,
                consumerTag,
                consumer,
                cancellationToken);

            _consumerTags.Add(finalTag);
            RabbitMQEventConsumerLogs.LogConsumerStartedForQueue(logger, queueConfig.Name, finalTag);
        }
        catch (Exception ex)
        {
            RabbitMQEventConsumerLogs.LogStartConsumerFailed(logger, ex, queueConfig.Name);
            throw;
        }
    }

    private async Task ProcessMessage(BasicDeliverEventArgs ea, QueueConfiguration queueConfig)
    {
        string messageId = ea.BasicProperties.MessageId ?? Guid.NewGuid().ToString();
        string eventTypeName = ea.BasicProperties.Type ?? "Unknown";

        try
        {
            RabbitMQEventConsumerLogs.LogProcessingMessage(logger, messageId, eventTypeName, queueConfig.Name);

            string messageBody = Encoding.UTF8.GetString(ea.Body.ToArray());

            if (!eventRoutingRegistry.TryGetEventType(eventTypeName, out Type? eventType) || eventType is null)
            {
                RabbitMQEventConsumerLogs.LogUnknownEventType(logger, eventTypeName);
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, false);
                return;
            }

            object? eventObject = JsonSerializer.Deserialize(messageBody, eventType, _jsonOptions);
            if (eventObject == null)
            {
                RabbitMQEventConsumerLogs.LogDeserializationFailed(logger, eventTypeName);
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, false);
                return;
            }

            MessageContext context = CreateMessageContext(ea, queueConfig);
            await messageProcessor.ProcessAsync(eventObject, eventType, context);

            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            RabbitMQEventConsumerLogs.LogMessageProcessed(logger, messageId, eventTypeName);
        }
        catch (Exception ex)
        {
            RabbitMQEventConsumerLogs.LogProcessingFailed(logger, ex, messageId, eventTypeName, queueConfig.Name);
            await HandleMessageError(ea);
        }
    }

    private static MessageContext CreateMessageContext(BasicDeliverEventArgs ea, QueueConfiguration queueConfig)
    {
        MessageContext context = new()
        {
            MessageId = ea.BasicProperties.MessageId ?? Guid.NewGuid().ToString(),
            EventType = ea.BasicProperties.Type ?? "Unknown",
            QueueName = queueConfig.Name,
            ReceivedAt = DateTime.UtcNow,
            RetryCount = GetRetryCount(ea.BasicProperties)
        };

        if (ea.BasicProperties.Headers != null)
        {
            foreach (KeyValuePair<string, object?> header in ea.BasicProperties.Headers)
            {
                context.Headers[header.Key] = header.Value;
            }
        }

        return context;
    }

    private async Task HandleMessageError(BasicDeliverEventArgs ea)
    {
        RabbitMQEventConsumerLogs.LogMessageRequeued(logger);
        await _channel!.BasicNackAsync(ea.DeliveryTag, false, false);
    }

    private static int GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers != null &&
            properties.Headers.TryGetValue("x-retry-count", out object? retryCountObj))
        {
            if (retryCountObj is byte[] bytes)
            {
                return BitConverter.ToInt32(bytes, 0);
            }

            if (int.TryParse(retryCountObj?.ToString(), out int retryCount))
            {
                return retryCount;
            }
        }

        return 0;
    }

    #region Dispose Pattern

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            await StopAsync();
            if (_channel is not null)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }

            RabbitMQEventConsumerLogs.LogDisposed(logger);
        }
        catch (Exception ex)
        {
            RabbitMQEventConsumerLogs.LogDisposeError(logger, ex);
        }
        finally
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DisposeAsync().AsTask().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }

    ~RabbitMQEventConsumer()
    {
        Dispose();
    }

    #endregion
}