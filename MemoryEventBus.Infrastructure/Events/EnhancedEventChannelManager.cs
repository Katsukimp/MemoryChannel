using System.Diagnostics;
using MemoryEventBus.Domain.Events;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using MemoryEventBus.Infrastructure.Events.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MemoryEventBus.Infrastructure.Events
{
    public class EnhancedEventChannelManager : EventChannelManager
    {
        private readonly ILogger<EnhancedEventChannelManager> _logger;
        private readonly IEventBusMetrics? _metrics;
        private readonly EventBusActivitySource? _activitySource;

        public EnhancedEventChannelManager(ILogger<EnhancedEventChannelManager> logger, IEventBusMetrics? metrics = null, EventBusActivitySource? activitySource = null)
        {
            _logger = logger;
            _metrics = metrics;
            _activitySource = activitySource;
        }

        public override async Task<bool> TryWriteAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            Activity? activity = _activitySource?.StartPublishActivity(typeof(TEvent).Name);
            try
            {
                var result = await base.TryWriteAsync(@event, cancellationToken);
                if (result)
                {
                    _metrics?.RecordEventPublished<TEvent>(typeof(TEvent).Name);
                    activity?.SetTag("eventbus.publish.success", true);
                    _logger.LogDebug("Successfully published event {EventType} with ID {EventId}", typeof(TEvent).Name, @event.EventId);
                }
                else
                {
                    activity?.SetTag("eventbus.publish.success", false);
                    _logger.LogWarning("Failed to publish event {EventType} with ID {EventId}", typeof(TEvent).Name, @event.EventId);
                    _metrics?.RecordEventFailed<TEvent>(typeof(TEvent).Name, "ChannelWriteFailed");
                }
                return result;
            }
            catch (Exception ex)
            {
                activity?.SetTag("eventbus.publish.error", ex.GetType().Name);
                _logger.LogError(ex, "Error publishing event {EventType} with ID {EventId}", typeof(TEvent).Name, @event.EventId);
                _metrics?.RecordEventFailed<TEvent>(typeof(TEvent).Name, ex.GetType().Name);
                return false;
            }
            finally
            {
                activity?.Stop();
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