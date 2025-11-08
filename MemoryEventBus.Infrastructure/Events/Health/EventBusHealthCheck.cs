using MemoryEventBus.Domain.Events.Interfaces.Base;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MemoryEventBus.Infrastructure.Events.Health
{
 public sealed class EventBusHealthCheck : IHealthCheck
 {
 private readonly IEventChannelManager _manager;
 private readonly IEventBusMetrics? _metrics;
 private readonly EventBusHealthOptions _options;

 public EventBusHealthCheck(IEventChannelManager manager, IEventBusMetrics? metrics, EventBusHealthOptions options)
 {
 _manager = manager;
 _metrics = metrics;
 _options = options;
 }

 public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
 {
 // Simple check based on number of channels and depth of a few known channels if metrics recorded it elsewhere.
 var channelCount = _manager.GetChannelCount();
 if (channelCount > _options.MaxChannelsWarningThreshold)
 {
 return Task.FromResult(HealthCheckResult.Degraded($"EventBus has high channel count: {channelCount}"));
 }

 // Best-effort depth probe for a generic DomainEvent channel is not possible; users should create per-event checks.
 // For now we report Healthy if channel count is within threshold.
 return Task.FromResult(HealthCheckResult.Healthy($"EventBus OK. Channels: {channelCount}"));
 }
 }
}
