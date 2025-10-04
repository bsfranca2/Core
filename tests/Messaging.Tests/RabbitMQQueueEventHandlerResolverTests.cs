namespace Bsfranca2.Messaging.Tests;

public class RabbitMQQueueEventHandlerResolverTests
{
    [Fact]
    public void GetEventTypesForQueue_ReturnsMatchingEvents()
    {
        MessagingOptions options = new();
        options.Queues.Add(new QueueConfiguration
        {
            Name = "orders-queue",
            ExchangeName = "orders",
            RoutingKey = "orders.created"
        });

        options.EventRouting[typeof(OrderCreatedEvent)] = new("orders", "orders.created");
        options.EventRouting[typeof(OrderCancelledEvent)] = new("orders", "orders.cancelled");

        RabbitMQQueueEventHandlerResolver resolver = new(options);

        Type[] eventTypes = resolver.GetEventTypesForQueue("ORDERS-QUEUE").ToArray();

        Assert.Single(eventTypes);
        Assert.Equal(typeof(OrderCreatedEvent), eventTypes[0]);
    }

    [Fact]
    public void GetEventTypesForQueue_ReturnsEmptyWhenQueueMissing()
    {
        RabbitMQQueueEventHandlerResolver resolver = new(new MessagingOptions());

        Assert.Empty(resolver.GetEventTypesForQueue("unknown"));
    }

    [Fact]
    public void GetEventTypesForQueue_ReturnsEmptyWhenQueueHasNoRouting()
    {
        MessagingOptions options = new();
        options.Queues.Add(new QueueConfiguration
        {
            Name = "orders-queue"
        });

        RabbitMQQueueEventHandlerResolver resolver = new(options);

        Assert.Empty(resolver.GetEventTypesForQueue("orders-queue"));
    }

    private sealed class OrderCreatedEvent : IEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }

    private sealed class OrderCancelledEvent : IEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}
