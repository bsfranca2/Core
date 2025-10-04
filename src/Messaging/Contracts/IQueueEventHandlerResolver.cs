namespace Bsfranca2.Messaging.Contracts;

public interface IQueueEventHandlerResolver
{
    IEnumerable<Type> GetEventTypesForQueue(string queueName);
}