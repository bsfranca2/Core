using Bsfranca2.Core;
using Bsfranca2.Messaging.Configurations;

namespace Bsfranca2.Messaging.Contracts;

public interface IEventRoutingRegistry
{
    EventRouting GetRouting(Type eventType);
    EventRouting GetRouting<T>() where T : class, IEvent;
    bool TryGetEventType(string eventTypeName, out Type? eventType);
}