namespace MemoryEventBus.Domain.Events.Aggregate
{
    public class DomainEvent
    {
        public virtual required string EventId { get; set; }
        public virtual required string EventName { get; set; }
        public virtual required DateTime OccurredOn { get; set; }
    }
}
