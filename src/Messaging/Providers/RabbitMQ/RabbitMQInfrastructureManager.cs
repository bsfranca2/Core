using Bsfranca2.Messaging.Configurations;
using Bsfranca2.Messaging.Contracts;

using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

internal sealed class RabbitMQInfrastructureManager(
    ILogger<RabbitMQInfrastructureManager> logger,
    MessagingOptions options
) : IInfrastructureManager
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly HashSet<string> _declaredExchanges = new();
    private readonly HashSet<string> _declaredQueues = new();
    private readonly HashSet<string> _declaredBindings = new();

    public async Task EnsureInfrastructureAsync(IChannel channel, CancellationToken cancellationToken = default)
    {
        try
        {
            RabbitMQInfrastructureManagerLogs.LogSetupInfrastructureStart(logger);

            foreach (ExchangeConfiguration exchange in options.Exchanges)
            {
                await DeclareExchangeAsync(channel, exchange, cancellationToken);
            }

            foreach (QueueConfiguration queue in options.Queues)
            {
                await DeclareQueueAsync(channel, queue, cancellationToken);
                await CreateBindingAsync(channel, queue, cancellationToken);
            }

            RabbitMQInfrastructureManagerLogs.LogSetupInfrastructureComplete(logger);
        }
        catch (Exception ex)
        {
            RabbitMQInfrastructureManagerLogs.LogSetupInfrastructureError(logger, ex);
            throw;
        }
    }

    private async Task DeclareExchangeAsync(
        IChannel channel,
        ExchangeConfiguration exchangeConfig,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_declaredExchanges.Contains(exchangeConfig.Name))
            {
                return;
            }

            try
            {
                await channel.ExchangeDeclareAsync(
                    exchangeConfig.Name,
                    exchangeConfig.Type.ToString().ToLower(),
                    exchangeConfig.Durable,
                    exchangeConfig.AutoDelete,
                    exchangeConfig.Arguments,
                    cancellationToken: cancellationToken);

                _declaredExchanges.Add(exchangeConfig.Name);
                RabbitMQInfrastructureManagerLogs.LogDeclareExchange(logger, exchangeConfig.Name, exchangeConfig.Type);
            }
            catch (Exception ex)
            {
                RabbitMQInfrastructureManagerLogs.LogDeclareExchangeError(logger, ex, exchangeConfig.Name);
                throw;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task DeclareQueueAsync(
        IChannel channel,
        QueueConfiguration queueConfig,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_declaredQueues.Contains(queueConfig.Name))
            {
                return;
            }

            try
            {
                QueueDeclareOk result = await channel.QueueDeclareAsync(
                    queueConfig.Name,
                    queueConfig.Durable,
                    queueConfig.Exclusive,
                    queueConfig.AutoDelete,
                    queueConfig.Arguments,
                    cancellationToken: cancellationToken);

                _declaredQueues.Add(queueConfig.Name);
                RabbitMQInfrastructureManagerLogs.LogDeclareQueue(logger, queueConfig.Name, result.MessageCount,
                    result.ConsumerCount);
            }
            catch (Exception ex)
            {
                RabbitMQInfrastructureManagerLogs.LogDeclareQueueError(logger, ex, queueConfig.Name);
                throw;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task CreateBindingAsync(
        IChannel channel,
        QueueConfiguration queueConfig,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(queueConfig.ExchangeName) || string.IsNullOrEmpty(queueConfig.RoutingKey))
        {
            return;
        }

        string bindingKey = $"{queueConfig.Name}->{queueConfig.ExchangeName}|{queueConfig.RoutingKey}";

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_declaredBindings.Contains(bindingKey))
            {
                return;
            }

            try
            {
                await channel.QueueBindAsync(
                    queueConfig.Name,
                    queueConfig.ExchangeName,
                    queueConfig.RoutingKey,
                    cancellationToken: cancellationToken);

                _declaredBindings.Add(bindingKey);
                RabbitMQInfrastructureManagerLogs.LogCreateBinding(logger, queueConfig.Name, queueConfig.ExchangeName,
                    queueConfig.RoutingKey);
            }
            catch (Exception ex)
            {
                RabbitMQInfrastructureManagerLogs.LogCreateBindingError(logger, ex, queueConfig.Name, queueConfig.ExchangeName,
                    queueConfig.RoutingKey);
                throw;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}