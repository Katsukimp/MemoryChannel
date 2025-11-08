using MemoryEventBus.Domain.Events.Interfaces.Base;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MemoryEventBus.Infrastructure.Events.Health
{
    /// <summary>
    /// Basic health check assessing event bus channel counts.
    /// </summary>
    public sealed class EventBusHealthCheck : IHealthCheck
    {
        private readonly IEventChannelManager _manager;
        private readonly IEventBusMetrics? _metrics;
        private readonly EventBusHealthOptions _options;

        /// <summary>
        /// Creates a new <see cref="EventBusHealthCheck"/>.
        /// </summary>
        public EventBusHealthCheck(IEventChannelManager manager, IEventBusMetrics? metrics, EventBusHealthOptions options)
        {
            _manager = manager;
            _metrics = metrics;
            _options = options;
        }

        /// <inheritdoc />
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var channelCount = _manager.GetChannelCount();
            if (channelCount > _options.MaxChannelsWarningThreshold)
            {
                return Task.FromResult(HealthCheckResult.Degraded($"EventBus has high channel count: {channelCount}"));
            }
            return Task.FromResult(HealthCheckResult.Healthy($"EventBus OK. Channels: {channelCount}"));
        }
    }
}
