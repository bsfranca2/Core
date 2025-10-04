using RabbitMQ.Client;

using ExchangeType = Bsfranca2.Messaging.Configurations.ExchangeType;

namespace Bsfranca2.Messaging.Tests;

[Collection("RabbitMq")]
public sealed class RabbitMQInfrastructureManagerTests
{
    private readonly RabbitMqFixture _fixture;

    public RabbitMQInfrastructureManagerTests(RabbitMqFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EnsureInfrastructureAsync_DeclaresExchangeAndQueue()
    {
        MessagingOptions options = new();
        string exchangeName = $"orders-{Guid.NewGuid():N}";
        string queueName = $"orders-{Guid.NewGuid():N}";
        string routingKey = "orders.created";

        options.Exchanges.Add(new ExchangeConfiguration
        {
            Name = exchangeName,
            Type = ExchangeType.Topic
        });

        options.Queues.Add(new QueueConfiguration
        {
            Name = queueName,
            ExchangeName = exchangeName,
            RoutingKey = routingKey
        });

        RabbitMQInfrastructureManager manager = new(NullLogger<RabbitMQInfrastructureManager>.Instance, options);

        ConnectionFactory factory = new()
        {
            Uri = new Uri(_fixture.ConnectionString)
        };

        await using IConnection connection = await factory.CreateConnectionAsync();
        await using IChannel channel = await connection.CreateChannelAsync();

        await manager.EnsureInfrastructureAsync(channel);

        await channel.ExchangeDeclarePassiveAsync(exchangeName);
        QueueDeclareOk queueInfo = await channel.QueueDeclarePassiveAsync(queueName);
        Assert.Equal(queueName, queueInfo.QueueName);
    }

    [Fact]
    public async Task EnsureInfrastructureAsync_IsIdempotent()
    {
        MessagingOptions options = new();
        string exchangeName = $"inbox-{Guid.NewGuid():N}";
        string queueName = $"inbox-{Guid.NewGuid():N}";

        options.Exchanges.Add(new ExchangeConfiguration
        {
            Name = exchangeName,
            Type = ExchangeType.Direct
        });

        options.Queues.Add(new QueueConfiguration
        {
            Name = queueName,
            ExchangeName = exchangeName,
            RoutingKey = "inbox.received"
        });

        RabbitMQInfrastructureManager manager = new(NullLogger<RabbitMQInfrastructureManager>.Instance, options);

        ConnectionFactory factory = new()
        {
            Uri = new Uri(_fixture.ConnectionString)
        };

        await using IConnection connection = await factory.CreateConnectionAsync();
        await using IChannel channel = await connection.CreateChannelAsync();

        await manager.EnsureInfrastructureAsync(channel);
        await manager.EnsureInfrastructureAsync(channel);

        QueueDeclareOk queueInfo = await channel.QueueDeclarePassiveAsync(queueName);
        Assert.Equal(queueName, queueInfo.QueueName);
    }
}
