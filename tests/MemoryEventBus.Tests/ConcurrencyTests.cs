using System;
using System.Linq;
using System.Threading.Tasks;
using MemoryEventBus.Domain.Events;
using MemoryEventBus.Domain.Events.Aggregate;
using Xunit;

public class ConcurrencyTests
{
 private sealed class TestEvent : DomainEvent { }

 [Fact]
 public async Task Publish_Parallel_ShouldSucceed()
 {
 var manager = new EventChannelManager();
 manager.GetOrCreateChannel<TestEvent>();

 var tasks = Enumerable.Range(0,1000).Select(async i =>
 {
 var ok = await manager.TryWriteAsync(new TestEvent
 {
 EventId = i.ToString(),
 EventName = "Test",
 OccurredOn = DateTime.UtcNow
 });
 Assert.True(ok);
 });

 await Task.WhenAll(tasks);
 }
}
