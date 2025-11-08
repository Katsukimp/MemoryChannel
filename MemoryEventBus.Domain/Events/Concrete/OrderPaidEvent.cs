using MemoryEventBus.Domain.Entities;
using MemoryEventBus.Domain.Events.Aggregate;

namespace MemoryEventBus.Domain.Events.Concrete
{
    public class OrderPaidEvent : DomainEvent
    {
        public required Order Message { get; set; }
    }
}
