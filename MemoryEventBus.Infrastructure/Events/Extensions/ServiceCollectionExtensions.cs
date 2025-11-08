using MemoryEventBus.Domain.Events;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using MemoryEventBus.Infrastructure.Events.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace MemoryEventBus.Infrastructure.Events.Extensions
{
 public static class ServiceCollectionExtensions
 {
 public static IServiceCollection AddMemoryEventBus(this IServiceCollection services, bool enableMetrics = true)
 {
 services.AddSingleton<IEventChannelManager, EnhancedEventChannelManager>();
 if (enableMetrics)
 {
 services.AddSingleton<IEventBusMetrics, EventBusMetrics>();
 }
 else
 {
 services.AddSingleton<IEventBusMetrics, NullEventBusMetrics>();
 }
 return services;
 }
 }
}
