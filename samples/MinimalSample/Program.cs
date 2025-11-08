using System;
using System.Threading.Channels;
using MemoryEventBus.Domain.Events.Aggregate;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using MemoryEventBus.Infrastructure.Events.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
 .ConfigureServices(services =>
 {
 services.AddMemoryEventBus(enableMetrics: true);
 services.AddHostedService<SampleConsumer>();
 })
 .Build();

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

Console.WriteLine("Published 5 SampleEvent messages. Press Enter to exit.");
Console.ReadLine();

await host.StopAsync();

// Simple event type
public sealed class SampleEvent : DomainEvent { }

// Simple consumer
public sealed class SampleConsumer : BackgroundService
{
 private readonly Channel<DomainEvent> _channel;
 private readonly ILogger<SampleConsumer> _logger;

 public SampleConsumer(IEventChannelManager manager, ILogger<SampleConsumer> logger)
 {
 _channel = manager.GetOrCreateChannel<SampleEvent>();
 _logger = logger;
 }

 protected override async Task ExecuteAsync(CancellationToken stoppingToken)
 {
 await foreach (var ev in _channel.Reader.ReadAllAsync(stoppingToken))
 {
 if (ev is SampleEvent se)
 {
 _logger.LogInformation("Consumed {EventName} {EventId}", se.EventName, se.EventId);
 }
 }
 }
}
