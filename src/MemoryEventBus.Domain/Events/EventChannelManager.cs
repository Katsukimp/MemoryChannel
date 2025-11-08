using System.Collections.Concurrent;
using System.Threading.Channels;
using MemoryEventBus.Domain.Events.Aggregate;
using MemoryEventBus.Domain.Events.Interfaces.Base;

namespace MemoryEventBus.Domain.Events
{
    /// <summary>
    /// Default implementation of <see cref="IEventChannelManager"/> managing channels per event type.
    /// </summary>
    public class EventChannelManager : IEventChannelManager
    {
        private readonly ConcurrentDictionary<Type, object> _channels = new();
        
        /// <inheritdoc />
        public virtual Channel<TEvent> GetOrCreateChannel<TEvent>() where TEvent : DomainEvent
        {
            var channelObj = _channels.GetOrAdd(typeof(TEvent), _ => 
            {
                var channel = Channel.CreateUnbounded<TEvent>(new UnboundedChannelOptions
                {
                    SingleReader = false,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false
                });
                return channel;
            });

            return (Channel<TEvent>)channelObj;
        }

        /// <inheritdoc />
        public virtual Channel<TEvent> GetOrCreateBoundedChannel<TEvent>(int capacity) where TEvent : DomainEvent
        {
            var channelObj = _channels.GetOrAdd(typeof(TEvent), _ => 
            {
                var channel = Channel.CreateBounded<TEvent>(new BoundedChannelOptions(capacity)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = false,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false
                });
                return channel;
            });

            return (Channel<TEvent>)channelObj;
        }

        /// <inheritdoc />
        public virtual async ValueTask<bool> TryWriteAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : DomainEvent
        {
            if (_channels.TryGetValue(typeof(TEvent), out var channelObj) is false)
                return false;

            try
            {
                var channel = (Channel<TEvent>)channelObj;
                await channel.Writer.WriteAsync(@event, cancellationToken).ConfigureAwait(false);
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

        /// <inheritdoc />
        public virtual int GetChannelDepth<TEvent>() where TEvent : DomainEvent
        {
            if (_channels.TryGetValue(typeof(TEvent), out var channelObj))
            {
                var channel = (Channel<TEvent>)channelObj;
                var reader = channel.Reader;

                if (reader.CanCount)
                    return reader.Count;

                return -1;
            }
            return 0;
        }

        /// <inheritdoc />
        public virtual void CloseChannel<TEvent>() where TEvent : DomainEvent
        {
            if (_channels.TryRemove(typeof(TEvent), out var channelObj))
            {
                var channel = (Channel<TEvent>)channelObj;
                channel.Writer.Complete();
            }
        }

        /// <inheritdoc />
        public virtual int GetChannelCount() => _channels.Count;
    }
}
