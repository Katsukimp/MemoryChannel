using System.Diagnostics;

namespace MemoryEventBus.Infrastructure.Events.Diagnostics
{
 public sealed class EventBusActivitySource
 {
 public const string ActivitySourceName = "MemoryEventBus";
 private readonly ActivitySource _source;
 private readonly bool _enabled;

 public EventBusActivitySource(bool enabled)
 {
 _enabled = enabled;
 _source = new ActivitySource(ActivitySourceName);
 }

 public Activity? StartPublishActivity(string eventType)
 {
 if (!_enabled) return null;
 return _source.StartActivity("eventbus.publish", ActivityKind.Producer, default(ActivityContext));
 }

 public Activity? StartConsumeActivity(string eventType)
 {
 if (!_enabled) return null;
 return _source.StartActivity("eventbus.consume", ActivityKind.Consumer, default(ActivityContext));
 }
 }
}
