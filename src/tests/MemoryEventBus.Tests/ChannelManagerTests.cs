using System;
using System.Threading.Tasks;
using MemoryEventBus.Domain.Events;
using MemoryEventBus.Domain.Events.Aggregate;
using Xunit;

public class ChannelManagerTests
{
    private sealed class TestEvent : DomainEvent { }

    [Fact]
    public async Task TryWriteAsync_ShouldReturnFalse_WhenChannelNotCreated()
    {
        var manager = new EventChannelManager();
        var result = await manager.TryWriteAsync(new TestEvent
        {
            EventId = "1",
            EventName = "Test",
            OccurredOn = DateTime.UtcNow
        });

        Assert.False(result);
    }

    [Fact]
    public async Task TryWriteAsync_ShouldReturnTrue_AfterChannelCreated()
    {
        var manager = new EventChannelManager();
        manager.GetOrCreateChannel<TestEvent>();

        var result = await manager.TryWriteAsync(new TestEvent
        {
            EventId = "2",
            EventName = "Test",
            OccurredOn = DateTime.UtcNow
        });

        Assert.True(result);
    }

    [Fact]
    public void GetChannelDepth_ShouldReturnNonNegative()
    {
        var manager = new EventChannelManager();
        manager.GetOrCreateChannel<TestEvent>();

        var depth = manager.GetChannelDepth<TestEvent>();

        Assert.True(depth >=0);
    }
}
