namespace Bsfranca2.Messaging.Contracts;

public interface IHandlerRegistry
{
    void Register(Type eventType);
    bool HasHandlerFor(Type eventType);
}