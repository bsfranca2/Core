using System.Text;
using System.Text.Json;

using Bsfranca2.Core;
using Bsfranca2.Messaging.Configurations;
using Bsfranca2.Messaging.Contracts;

using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

internal sealed class RabbitMQEventPublisher(
    ILogger<RabbitMQEventPublisher> logger,
    RabbitMQConnectionFactory connectionFactory,
    IEventRoutingRegistry eventRoutingRegistry
) : IEventPublisher, IDisposable, IAsyncDisposable
{
    private readonly JsonSerializerOptions _jsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false };

    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed;

    private async Task EnsureConnectionAsync()
    {
        if (_connection is not { IsOpen: true } || _channel is not { IsOpen: true })
        {
            _connection = await connectionFactory.GetPublisherConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
        }
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        EventRouting routing = eventRoutingRegistry.GetRouting<T>();
        await PublishInternalAsync(@event, typeof(T), routing.ExchangeName, routing.RoutingKey, cancellationToken);
    }

    public async Task PublishAsync(object @event, Type eventType, CancellationToken cancellationToken = default)
    {
        EventRouting routing = eventRoutingRegistry.GetRouting(eventType);
        await PublishInternalAsync(@event, eventType, routing.ExchangeName, routing.RoutingKey, cancellationToken);
    }

    private async Task PublishInternalAsync(
        object message,
        Type messageType,
        string exchange,
        string routingKey,
        CancellationToken cancellationToken)
    {
        try
        {
            await EnsureConnectionAsync();

            RabbitMQEventPublisherLogs.LogPublishingEvent(logger, messageType.Name, exchange, routingKey);

            string messageJson = JsonSerializer.Serialize(message, messageType, _jsonOptions);
            ReadOnlyMemory<byte> body = Encoding.UTF8.GetBytes(messageJson);

            BasicProperties properties = new();
            ConfigureMessageProperties(properties, message, messageType);

            await _channel!.BasicPublishAsync(
                exchange,
                routingKey,
                false,
                properties,
                body,
                cancellationToken);

            RabbitMQEventPublisherLogs.LogPublishedEventSuccess(logger, messageType.Name, exchange);
        }
        catch (Exception ex)
        {
            RabbitMQEventPublisherLogs.LogPublishEventError(logger, ex, messageType.Name, exchange, routingKey);
            throw;
        }
    }

    private static void ConfigureMessageProperties(BasicProperties properties, object message, Type messageType)
    {
        properties.ContentType = "application/json";
        properties.DeliveryMode = DeliveryModes.Persistent;
        properties.Type = messageType.Name;
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        if (message is IEvent eventMessage)
        {
            properties.MessageId = eventMessage.EventId.ToString();
            properties.Headers = new Dictionary<string, object?>
            {
                ["EventId"] = eventMessage.EventId.ToString(),
                ["EventType"] = messageType.Name,
                ["OccurredOn"] = eventMessage.OccurredOnUtc.ToString("O")
            };
        }
        else
        {
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Headers = new Dictionary<string, object?>
            {
                ["MessageId"] = properties.MessageId,
                ["MessageType"] = messageType.Name,
                ["PublishedOn"] = DateTime.UtcNow.ToString("O")
            };
        }

        properties.Headers["Publisher"] = "RabbitMQEventPublisher";
        properties.Headers["Assembly"] = messageType.Assembly.GetName().Name ?? "Unknown";
        properties.Headers["Namespace"] = messageType.Namespace ?? "Unknown";
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
            if (_channel != null)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }

            RabbitMQEventPublisherLogs.LogDisposed(logger);
        }
        catch (Exception ex)
        {
            RabbitMQEventPublisherLogs.LogDisposeError(logger, ex);
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

    ~RabbitMQEventPublisher()
    {
        Dispose();
    }

    #endregion
}