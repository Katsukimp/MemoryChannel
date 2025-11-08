using MemoryEventBus.Domain.Events;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using MemoryEventBus.Infrastructure.Events.Diagnostics;
using MemoryEventBus.Infrastructure.Events.Health;
using MemoryEventBus.Infrastructure.Events.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace MemoryEventBus.Infrastructure.Events.Extensions
{
 public sealed class EventBusRegistrationOptions
 {
 public bool EnableMetrics { get; set; } = true;
 public bool EnableTracing { get; set; } = false;
 public bool EnableHealthChecks { get; set; } = false;
 public EventBusHealthOptions Health { get; set; } = new();
 }

 public static class ServiceCollectionExtensions
 {
 public static IServiceCollection AddMemoryEventBus(this IServiceCollection services, bool enableMetrics = true)
 {
 return services.AddMemoryEventBus(opts => opts.EnableMetrics = enableMetrics);
 }

 public static IServiceCollection AddMemoryEventBus(this IServiceCollection services, Action<EventBusRegistrationOptions> configure)
 {
 var opts = new EventBusRegistrationOptions();
 configure(opts);

 services.AddSingleton<IEventChannelManager, EnhancedEventChannelManager>();

 if (opts.EnableMetrics)
 {
 services.AddSingleton<IEventBusMetrics, EventBusMetrics>();
 }
 else
 {
 services.AddSingleton<IEventBusMetrics, NullEventBusMetrics>();
 }

 // Tracing
 services.AddSingleton(new EventBusActivitySource(enabled: opts.EnableTracing));

 // Health checks opt-in: we only register the check dependencies; the host app calls AddHealthChecks().AddCheck<EventBusHealthCheck>(...)
 if (opts.EnableHealthChecks)
 {
 services.AddSingleton(opts.Health);
 services.AddSingleton<EventBusHealthCheck>();
 }

 return services;
 }
 }
}
