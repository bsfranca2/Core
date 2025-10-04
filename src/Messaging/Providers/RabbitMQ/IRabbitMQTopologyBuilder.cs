using System;

using Bsfranca2.Core;
using Bsfranca2.Messaging.Configurations;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

public interface IRabbitMQTopologyBuilder
{
    IRabbitMQTopologyBuilder AddExchange(ExchangeConfiguration exchange);
    IRabbitMQTopologyBuilder AddQueue(QueueConfiguration queue);
    IRabbitMQTopologyBuilder MapEvent<TEvent>(string exchangeName, string routingKey) where TEvent : class, IEvent;
    IRabbitMQTopologyBuilder MapEvent(Type eventType, string exchangeName, string routingKey);
}

