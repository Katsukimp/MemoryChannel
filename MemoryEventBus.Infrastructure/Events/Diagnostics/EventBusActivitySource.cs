using System.Diagnostics;

namespace MemoryEventBus.Infrastructure.Events.Diagnostics;
/// <summary>
/// Provides ActivitySource-based tracing for publish and consume operations.
/// </summary>
public sealed class EventBusActivitySource
{
 /// <summary>Name of the ActivitySource used for event bus spans.</summary>
 public const string ActivitySourceName = "MemoryEventBus";
 private readonly ActivitySource _source;
 private readonly bool _enabled;

 /// <summary>
 /// Creates a new <see cref="EventBusActivitySource"/>.
 /// </summary>
 /// <param name="enabled">Whether tracing is enabled.</param>
 public EventBusActivitySource(bool enabled)
 {
 _enabled = enabled;
 _source = new ActivitySource(ActivitySourceName);
 }

 /// <summary>
 /// Starts a publish activity for the given event type or returns null if tracing disabled.
 /// </summary>
 public Activity? StartPublishActivity(string eventType)
 {
 if (!_enabled) return null;
 return _source.StartActivity("eventbus.publish", ActivityKind.Producer, default(ActivityContext));
 }

 /// <summary>
 /// Starts a consume activity for the given event type or returns null if tracing disabled.
 /// </summary>
 public Activity? StartConsumeActivity(string eventType)
 {
 if (!_enabled) return null;
 return _source.StartActivity("eventbus.consume", ActivityKind.Consumer, default(ActivityContext));
 }
}