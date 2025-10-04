using System;
using System.Collections.Generic;

using Bsfranca2.Core;
using Bsfranca2.Messaging.Configurations;
using Bsfranca2.Messaging.Contracts;

using Microsoft.Extensions.Logging;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

internal sealed class RabbitMQEventRoutingRegistry : IEventRoutingRegistry
{
    private readonly Dictionary<Type, EventRouting> _routingByType;
    private readonly Dictionary<string, Type> _typeByName;

    public RabbitMQEventRoutingRegistry(
        ILogger<RabbitMQEventRoutingRegistry> logger,
        MessagingOptions options)
    {
        Dictionary<Type, EventRouting> routingByType = [];
        Dictionary<string, Type> typeByName = [];

        foreach ((Type eventType, EventRouting routing) in options.EventRouting)
        {
            routingByType[eventType] = routing;

            if (!typeByName.TryAdd(eventType.Name, eventType))
            {
                Type conflictingType = typeByName[eventType.Name];
                throw new InvalidOperationException(
                    $"Event type name collision. The name '{eventType.Name}' is used by both '{conflictingType.FullName}' and '{eventType.FullName}'.");
            }
        }

        _routingByType = routingByType;
        _typeByName = typeByName;

        logger.LogDebug("Successfully registered {EventCount} event types.", routingByType.Count);
    }

    public EventRouting GetRouting(Type eventType)
    {
        if (!_routingByType.TryGetValue(eventType, out EventRouting? routing))
        {
            throw new KeyNotFoundException(
                $"No routing registered for event type '{eventType.FullName}'. Ensure it's in the scanned domain assembly.");
        }

        return routing;
    }

    public EventRouting GetRouting<T>() where T : class, IEvent
    {
        return GetRouting(typeof(T));
    }

    public bool TryGetEventType(string eventTypeName, out Type? eventType)
    {
        return _typeByName.TryGetValue(eventTypeName, out eventType);
    }
}
