using MemoryEventBus.Domain.Entities.Base;
using MemoryEventBus.Domain.Events.Aggregate;
using MemoryEventBus.Domain.Events.Concrete;

namespace MemoryEventBus.Domain.Entities
{
    public class Order : BaseModel
    {
        private List<DomainEvent> _events = [];
        private bool IsPayed { get; set; } = false;
        public decimal TotalAmount { get; set; } = decimal.Zero;

        public void Pay(decimal amount)
        {
            if(amount < TotalAmount)
                throw new AggregateException("Insufficient amount to pay the order.");

            IsPayed = true;
            _events.Add(new OrderPaidEvent
            {
                EventId = Id.ToString() + CreatedAt.ToString(),
                EventName = "OrderPaid",
                OccurredOn = DateTime.UtcNow,
                Message = this
            });
        }

        public bool GetIsPayed() => IsPayed;
        public IReadOnlyCollection<DomainEvent> GetDomainEvents() => _events.AsReadOnly();
        public void ClearEvents() => _events.Clear();
    }
}
