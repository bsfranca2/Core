using System;
using System.Collections.Generic;
using System.Linq;

using Bsfranca2.Messaging.Configurations;
using Bsfranca2.Messaging.Contracts;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

public sealed class RabbitMQQueueEventHandlerResolver(MessagingOptions options) : IQueueEventHandlerResolver
{
    private readonly MessagingOptions _options = options;

    public IEnumerable<Type> GetEventTypesForQueue(string queueName)
    {
        QueueConfiguration? queueConfig = _options.Queues
            .FirstOrDefault(q => string.Equals(q.Name, queueName, StringComparison.OrdinalIgnoreCase));

        if (queueConfig is null ||
            string.IsNullOrWhiteSpace(queueConfig.ExchangeName) ||
            string.IsNullOrWhiteSpace(queueConfig.RoutingKey))
        {
            return Enumerable.Empty<Type>();
        }

        IEnumerable<Type> eventTypes = _options.EventRouting
            .Where(mapping =>
                string.Equals(mapping.Value.ExchangeName, queueConfig.ExchangeName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(mapping.Value.RoutingKey, queueConfig.RoutingKey, StringComparison.OrdinalIgnoreCase))
            .Select(mapping => mapping.Key);

        return eventTypes.ToList();
    }
}
