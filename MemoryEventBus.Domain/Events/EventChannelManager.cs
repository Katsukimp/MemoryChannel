using MemoryEventBus.Domain.Events.Aggregate;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace MemoryEventBus.Domain.Events
{
    public class EventChannelManager : IEventChannelManager
    {
        private readonly ConcurrentDictionary<Type, Channel<DomainEvent>> _channels = new();
        
        public Channel<DomainEvent> GetOrCreateChannel<TEvent>() where TEvent : DomainEvent
        {
            return _channels.GetOrAdd(typeof(TEvent), _ => 
            {
                var channel = Channel.CreateUnbounded<DomainEvent>(new UnboundedChannelOptions
                {
                    SingleReader = false,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false
                });
                
                return channel;
            });
        }

        public Channel<DomainEvent> GetOrCreateBoundedChannel<TEvent>(int capacity) where TEvent : DomainEvent
        {
            return _channels.GetOrAdd(typeof(TEvent), _ => 
            {
                var channel = Channel.CreateBounded<DomainEvent>(new BoundedChannelOptions(capacity)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = false,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false
                });
                
                return channel;
            });
        }

        public async Task<bool> TryWriteAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : DomainEvent
        {
            if (_channels.TryGetValue(typeof(TEvent), out var channel) is false)
                return false;

            try
            {
                await channel.Writer.WriteAsync(@event, cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int GetChannelDepth<TEvent>() where TEvent : DomainEvent
        {
            if (_channels.TryGetValue(typeof(TEvent), out var channel))
            {
                var reader = channel.Reader;

                if (reader.CanCount)
                    return reader.Count;

                return -1;
            }
            return 0;
        }

        public void CloseChannel<TEvent>() where TEvent : DomainEvent
        {
            if (_channels.TryRemove(typeof(TEvent), out var channel))
                channel.Writer.Complete();
        }

        public int GetChannelCount() => _channels.Count;
    }
}
