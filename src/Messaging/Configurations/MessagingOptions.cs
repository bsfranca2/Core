namespace Bsfranca2.Messaging.Configurations;

public class MessagingOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public List<ExchangeConfiguration> Exchanges { get; set; } = new();
    public List<QueueConfiguration> Queues { get; set; } = new();
}

public class ExchangeConfiguration
{
    public string Name { get; set; } = string.Empty;
    public ExchangeType Type { get; set; } = ExchangeType.Topic;
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public Dictionary<string, object?> Arguments { get; set; } = new();
}

public class QueueConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; } = false;
    public bool AutoDelete { get; set; } = false;
    public Dictionary<string, object?> Arguments { get; set; } = new();
    public int? PrefetchCount { get; set; } = null; 
}