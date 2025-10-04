namespace Bsfranca2.Messaging.Tests;

public class RabbitMQTopologyBuilderTests
{
    private readonly MessagingOptions _options = new();

    private RabbitMQTopologyBuilder CreateBuilder()
    {
        return new RabbitMQTopologyBuilder(_options);
    }

    [Fact]
    public void AddExchange_ReplacesExistingConfigurationWithSameName()
    {
        _options.Exchanges.Add(new ExchangeConfiguration
        {
            Name = "orders",
            Type = ExchangeType.Fanout,
            Durable = false
        });

        ExchangeConfiguration updated = new()
        {
            Name = "orders",
            Type = ExchangeType.Topic,
            Durable = true
        };

        CreateBuilder().AddExchange(updated);

        ExchangeConfiguration result = Assert.Single(_options.Exchanges);
        Assert.Equal(ExchangeType.Topic, result.Type);
        Assert.True(result.Durable);
    }

    [Fact]
    public void AddQueue_ReplacesExistingConfigurationWithSameName()
    {
        _options.Queues.Add(new QueueConfiguration
        {
            Name = "orders-queue",
            ExchangeName = "orders",
            RoutingKey = "orders.created",
            Durable = false
        });

        QueueConfiguration updated = new()
        {
            Name = "orders-queue",
            ExchangeName = "orders",
            RoutingKey = "orders.created",
            Durable = true
        };

        CreateBuilder().AddQueue(updated);

        QueueConfiguration result = Assert.Single(_options.Queues);
        Assert.True(result.Durable);
    }

    [Fact]
    public void MapEvent_RegistersRoutingForEventType()
    {
        RabbitMQTopologyBuilder builder = CreateBuilder();

        builder.MapEvent<TestEvent>("orders", "orders.created");

        EventRouting routing = _options.EventRouting[typeof(TestEvent)];
        Assert.Equal("orders", routing.ExchangeName);
        Assert.Equal("orders.created", routing.RoutingKey);
    }

    [Fact]
    public void MapEvent_ThrowsWhenTypeDoesNotImplementIEvent()
    {
        RabbitMQTopologyBuilder builder = CreateBuilder();

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            builder.MapEvent(typeof(string), "orders", "orders.created"));

        Assert.Contains(nameof(IEvent), exception.Message);
    }

    private sealed class TestEvent : IEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}
