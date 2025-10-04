namespace Bsfranca2.Core;

public interface IEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}