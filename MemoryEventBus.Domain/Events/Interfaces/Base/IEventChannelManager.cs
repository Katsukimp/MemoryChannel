using MemoryEventBus.Domain.Events.Aggregate;
using System.Threading.Channels;

namespace MemoryEventBus.Domain.Events.Interfaces.Base
{
    public interface IEventChannelManager
    {
        Channel<DomainEvent> GetOrCreateChannel<TEvent>() where TEvent : DomainEvent;
        Channel<DomainEvent> GetOrCreateBoundedChannel<TEvent>(int capacity) where TEvent : DomainEvent;
        void CloseChannel<TEvent>() where TEvent : DomainEvent;
        int GetChannelCount();
        int GetChannelDepth<TEvent>() where TEvent : DomainEvent;
        Task<bool> TryWriteAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : DomainEvent;
    }
}
