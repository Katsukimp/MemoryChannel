using MemoryEventBus.Domain.Events.Interfaces.Base;
using MemoryEventBus.Infrastructure.Events.Diagnostics;
using MemoryEventBus.Infrastructure.Events.Health;
using MemoryEventBus.Infrastructure.Events.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace MemoryEventBus.Infrastructure.Events.Extensions;
/// <summary>
/// Options controlling which auxiliary features are enabled when registering the event bus.
/// </summary>
public sealed class EventBusRegistrationOptions
{
 /// <summary>Enable metrics collection via <see cref="IEventBusMetrics"/>.</summary>
 public bool EnableMetrics { get; set; } = true;
 /// <summary>Enable tracing via <see cref="EventBusActivitySource"/>.</summary>
 public bool EnableTracing { get; set; } = false;
 /// <summary>Enable registration of health check dependencies (host must add the health check).</summary>
 public bool EnableHealthChecks { get; set; } = false;
 /// <summary>Health check evaluation thresholds.</summary>
 public EventBusHealthOptions Health { get; set; } = new();
}

/// <summary>
/// Extension methods to register the memory event bus in a DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
 /// <summary>
 /// Registers the event bus with optional metrics.
 /// </summary>
 public static IServiceCollection AddMemoryEventBus(this IServiceCollection services, bool enableMetrics = true)
 => services.AddMemoryEventBus(opts => opts.EnableMetrics = enableMetrics);

 /// <summary>
 /// Registers the event bus using a configuration delegate for advanced options.
 /// </summary>
 public static IServiceCollection AddMemoryEventBus(this IServiceCollection services, Action<EventBusRegistrationOptions> configure)
 {
 var opts = new EventBusRegistrationOptions();
 configure(opts);

 services.AddSingleton<IEventChannelManager, EnhancedEventChannelManager>();

 if (opts.EnableMetrics)
 services.AddSingleton<IEventBusMetrics, EventBusMetrics>();
 else
 services.AddSingleton<IEventBusMetrics, NullEventBusMetrics>();

 // Tracing
 services.AddSingleton(new EventBusActivitySource(enabled: opts.EnableTracing));

 // Health checks opt-in
 if (opts.EnableHealthChecks)
 {
 services.AddSingleton(opts.Health);
 services.AddSingleton<EventBusHealthCheck>();
 }

 return services;
 }
}
