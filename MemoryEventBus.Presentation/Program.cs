using MemoryEventBus.Domain.Events;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using MemoryEventBus.Infrastructure.Events.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

// Minimal registration to ensure build without domain-specific sample code
builder.Services.AddSingleton<IEventChannelManager, EventChannelManager>();
builder.Services.AddSingleton<IEventBusMetrics, EventBusMetrics>();

var app = builder.Build();
app.MapGet("/", () => "MemoryEventBus sample host");
app.Run();
