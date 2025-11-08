using MemoryEventBus.Domain.Events;
using MemoryEventBus.Domain.Events.Aggregate;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using Microsoft.Extensions.Logging;

namespace MemoryEventBus.Infrastructure.Events
{
    public class EnhancedEventChannelManager : EventChannelManager
    {
        private readonly ILogger<EnhancedEventChannelManager> _logger;
        private readonly IEventBusMetrics? _metrics;

        public EnhancedEventChannelManager(ILogger<EnhancedEventChannelManager> logger, IEventBusMetrics? metrics = null)
        {
            _logger = logger;
            _metrics = metrics;
        }

        public override async Task<bool> TryWriteAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await base.TryWriteAsync(@event, cancellationToken);
                if (result)
                {
                    _metrics?.RecordEventPublished<TEvent>(typeof(TEvent).Name);
                    _logger.LogDebug("Successfully published event {EventType} with ID {EventId}", typeof(TEvent).Name, @event.EventId);
                }
                else
                {
                    _logger.LogWarning("Failed to publish event {EventType} with ID {EventId}", typeof(TEvent).Name, @event.EventId);
                    _metrics?.RecordEventFailed<TEvent>(typeof(TEvent).Name, "ChannelWriteFailed");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event {EventType} with ID {EventId}", typeof(TEvent).Name, @event.EventId);
                _metrics?.RecordEventFailed<TEvent>(typeof(TEvent).Name, ex.GetType().Name);
                return false;
            }
        }

        public override int GetChannelDepth<TEvent>()
        {
            var depth = base.GetChannelDepth<TEvent>();
            _metrics?.RecordChannelDepth(typeof(TEvent).Name, depth);
            return depth;
        }
    }
}