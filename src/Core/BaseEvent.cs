namespace Bsfranca2.Core;

public abstract record BaseEvent : IEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}