using System.Threading.Channels;
using MemoryEventBus.Domain.Events.Aggregate;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using MemoryEventBus.Infrastructure.Events.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
 .ConfigureServices(services =>
 {
 services.AddMemoryEventBus(enableMetrics: true);
 services.AddHostedService<SampleConsumer>();
 }).Build();

var publisher = host.Services.GetRequiredService<IEventChannelManager>();

await host.StartAsync();

for (int i =0; i <5; i++)
{
 await publisher.TryWriteAsync(new SampleEvent
 {
 EventId = Guid.NewGuid().ToString(),
 EventName = "SampleEvent",
 OccurredOn = DateTime.UtcNow
 });
}

Console.WriteLine("Published5 SampleEvent messages. Press Enter to exit.");
Console.ReadLine();

await host.StopAsync();

public sealed class SampleEvent : DomainEvent { }

public sealed class SampleConsumer(IEventChannelManager manager, ILogger<SampleConsumer> logger) : BackgroundService
{
 private readonly Channel<SampleEvent> _channel = manager.GetOrCreateChannel<SampleEvent>();
 private readonly ILogger<SampleConsumer> _logger = logger;

 protected override async Task ExecuteAsync(CancellationToken stoppingToken)
 {
 await foreach (var se in _channel.Reader.ReadAllAsync(stoppingToken))
 {
 _logger.LogInformation("Consumed {EventName} {EventId}", se.EventName, se.EventId);
 }
 }
}
