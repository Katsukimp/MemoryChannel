namespace MemoryEventBus.Domain.Entities.Base
{
    public class BaseModel
    {
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        public virtual bool Active { get; set; } = true;
        public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
