using System;
using System.Linq;

using Bsfranca2.Core;
using Bsfranca2.Messaging.Configurations;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

internal sealed class RabbitMQTopologyBuilder(MessagingOptions options) : IRabbitMQTopologyBuilder
{
    private readonly MessagingOptions _options = options;

    public IRabbitMQTopologyBuilder AddExchange(ExchangeConfiguration exchange)
    {
        ArgumentNullException.ThrowIfNull(exchange);
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange.Name, nameof(exchange.Name));

        ExchangeConfiguration? existing = _options.Exchanges
            .FirstOrDefault(e => string.Equals(e.Name, exchange.Name, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            _options.Exchanges.Remove(existing);
        }

        _options.Exchanges.Add(exchange);
        return this;
    }

    public IRabbitMQTopologyBuilder AddQueue(QueueConfiguration queue)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentException.ThrowIfNullOrWhiteSpace(queue.Name, nameof(queue.Name));

        QueueConfiguration? existing = _options.Queues
            .FirstOrDefault(q => string.Equals(q.Name, queue.Name, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            _options.Queues.Remove(existing);
        }

        _options.Queues.Add(queue);
        return this;
    }

    public IRabbitMQTopologyBuilder MapEvent<TEvent>(string exchangeName, string routingKey)
        where TEvent : class, IEvent
    {
        return MapEvent(typeof(TEvent), exchangeName, routingKey);
    }

    public IRabbitMQTopologyBuilder MapEvent(Type eventType, string exchangeName, string routingKey)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        if (!typeof(IEvent).IsAssignableFrom(eventType))
        {
            throw new ArgumentException(
                $"The event type '{eventType.FullName}' must implement {nameof(IEvent)}.",
                nameof(eventType));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(exchangeName, nameof(exchangeName));
        ArgumentException.ThrowIfNullOrWhiteSpace(routingKey, nameof(routingKey));

        EventRouting routing = new(exchangeName, routingKey);
        _options.EventRouting[eventType] = routing;

        return this;
    }
}

