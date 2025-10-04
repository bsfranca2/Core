using Bsfranca2.Messaging.Configurations;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

internal static class RabbitMQMessagingTopology
{
    internal static readonly Dictionary<Type, EventRouting> EventMappings = new()
    {
        [typeof(OrderPaymentRequestedEvent)] = new EventRouting("order.events", "order.payment.requested")
    };

    internal static IEnumerable<Type> GetAllEventTypes()
    {
        return EventMappings.Keys;
    }

    internal static readonly List<QueueConfiguration> QueueConfigurations =
    [
        new()
        {
            Name = "process-order-payment",
            ExchangeName = "order.events",
            RoutingKey = "order.payment.requested",
            Durable = true,
            Exclusive = false,
            AutoDelete = false,
            PrefetchCount = 3,
            Arguments = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", "order.dlx" },
                { "x-dead-letter-routing-key", "order.payment.requested.dead" },
                { "x-message-ttl", 300000 }, // 5 minutes
                { "x-max-retries", 3 }
            }
        },
        new()
        {
            Name = "process-order-payment.dead",
            ExchangeName = "order.dlx",
            RoutingKey = "order.payment.requested.dead",
            Durable = true,
            Exclusive = false,
            AutoDelete = false,
            Arguments = new Dictionary<string, object?>
            {
                { "x-message-ttl", 86400000 } // 24 hours
            }
        }
    ];

    internal static readonly List<ExchangeConfiguration> ExchangeConfigurations =
    [
        new() { Name = "order.events", Type = ExchangeType.Topic, Durable = true },
        new() { Name = "order.dlx", Type = ExchangeType.Direct, Durable = true }
    ];
}