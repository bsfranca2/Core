using System;

using Bsfranca2.Messaging.Configurations;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

public interface IRabbitMQRegistrationContext
{
    string ConnectionString { get; set; }
    IRabbitMQRegistrationContext WithInfrastructureSetup(bool enable = true);
    IRabbitMQRegistrationContext ConfigureTopology(Action<IRabbitMQTopologyBuilder> configure);
}

internal sealed class RabbitMQRegistrationContext(MessagingOptions options) : IRabbitMQRegistrationContext
{
    private readonly RabbitMQTopologyBuilder _topologyBuilder = new(options);

    public string ConnectionString { get; set; } = string.Empty;
    public bool SetupInfrastructure { get; set; }

    public IRabbitMQRegistrationContext WithInfrastructureSetup(bool enable = true)
    {
        SetupInfrastructure = enable;
        return this;
    }

    public IRabbitMQRegistrationContext ConfigureTopology(Action<IRabbitMQTopologyBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_topologyBuilder);
        return this;
    }
}
