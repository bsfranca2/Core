namespace Bsfranca2.Messaging.Providers.RabbitMQ;

public interface IRabbitMQRegistrationContext
{
    public string ConnectionString { get; set; }
    IRabbitMQRegistrationContext WithInfrastructureSetup(bool enable = true);
}

internal class RabbitMQRegistrationContext : IRabbitMQRegistrationContext
{
    public string ConnectionString { get; set; } = string.Empty;
    public bool SetupInfrastructure { get; set; }

    public IRabbitMQRegistrationContext WithInfrastructureSetup(bool enable = true)
    {
        SetupInfrastructure = enable;
        return this;
    }
}