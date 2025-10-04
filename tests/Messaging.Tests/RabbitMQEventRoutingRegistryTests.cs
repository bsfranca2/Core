namespace Bsfranca2.Messaging.Tests;

public class RabbitMQEventRoutingRegistryTests
{
    [Fact]
    public void Constructor_BuildsLookupFromOptions()
    {
        MessagingOptions options = new();
        EventRouting routing = new("orders", "orders.created");
        options.EventRouting[typeof(TestEvent)] = routing;

        RabbitMQEventRoutingRegistry registry = new(NullLogger<RabbitMQEventRoutingRegistry>.Instance, options);

        EventRouting resolved = registry.GetRouting(typeof(TestEvent));
        Assert.Equal(routing, resolved);

        bool found = registry.TryGetEventType(nameof(TestEvent), out Type? resolvedType);
        Assert.True(found);
        Assert.Equal(typeof(TestEvent), resolvedType);
    }

    [Fact]
    public void Constructor_ThrowsWhenDuplicateEventTypeNames()
    {
        MessagingOptions options = new();
        options.EventRouting[typeof(DuplicateNameEvent)] = new("orders", "orders.created");
        options.EventRouting[typeof(AlternateNamespace.DuplicateNameEvent)] = new("orders", "orders.updated");

        Assert.Throws<InvalidOperationException>(() =>
            new RabbitMQEventRoutingRegistry(NullLogger<RabbitMQEventRoutingRegistry>.Instance, options));
    }

    [Fact]
    public void GetRouting_ThrowsWhenEventNotRegistered()
    {
        RabbitMQEventRoutingRegistry registry = new(NullLogger<RabbitMQEventRoutingRegistry>.Instance, new MessagingOptions());

        Assert.Throws<KeyNotFoundException>(() => registry.GetRouting(typeof(TestEvent)));
    }

    private sealed class TestEvent : IEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }

    private sealed class DuplicateNameEvent : IEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }

    private static class AlternateNamespace
    {
        internal sealed class DuplicateNameEvent : IEvent
        {
            public Guid EventId { get; } = Guid.NewGuid();
            public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
        }
    }
}
