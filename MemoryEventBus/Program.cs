using MemoryEventBus.Application.UseCases;
using MemoryEventBus.Domain.Events;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using MemoryEventBus.Domain.Events.Interfaces.Producers;
using MemoryEventBus.Domain.UseCases;
using MemoryEventBus.Infrastructure.Events.Consumers;
using MemoryEventBus.Infrastructure.Events.ErrorHandling;
using MemoryEventBus.Infrastructure.Events.Metrics;
using MemoryEventBus.Infrastructure.Events.Producers.V1;
using MemoryEventBus.Infrastructure.Events.Producers.V2;
using MemoryEventBus.Infrastructure.Events.RetryPolicies;
using MemoryEventBus.Presentation.Formatter.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();

    if (builder.Environment.IsDevelopment())
    {
        logging.AddConsole(options =>
        {
            options.FormatterName = "custom";
        })
        .AddConsoleFormatter<CustomConsoleFormatter, CustomConsoleFormatterOptions>(options =>
        {
            options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ";
        });

        logging.AddDebug();
    }
    else
    {
        logging.AddJsonConsole(options =>
        {
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
            options.IncludeScopes = true;
            options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions
            {
                Indented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        });
    }

    logging.SetMinimumLevel(LogLevel.Information);
    logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
    logging.AddFilter("MemoryEventBus", LogLevel.Debug);
    logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
});

var useEnhancedFeatures = builder.Configuration.GetValue<bool>("EventBus:UseEnhancedFeatures", false);
if (useEnhancedFeatures)
{
    builder.Services.AddSingleton<IEventChannelManager, EventChannelManager>();
    builder.Services.AddSingleton<IEventBusMetrics, EventBusMetrics>();
    builder.Services.AddSingleton<IEventBusErrorHandler, DefaultEventBusErrorHandler>();
    builder.Services.AddSingleton<IRetryPolicy>(provider =>
        new ExponentialBackoffRetryPolicy(
            maxAttempts: 5,
            baseDelay: TimeSpan.FromMilliseconds(200),
            maxDelay: TimeSpan.FromSeconds(60)
        )
    );
    builder.Services.AddScoped<IOrderPaidProducer, EnhancedOrderPaidProducer>();
    builder.Services.AddHostedService<EnhancedStockReducerConsumer>();
}
else
{
    builder.Services.AddSingleton<IEventChannelManager, EventChannelManager>();
    builder.Services.AddScoped<IOrderPaidProducer, OrderPaidProducer>();
    builder.Services.AddHostedService<StockReducerConsumer>();
}

builder.Services.AddScoped<IPayUseCase, PayUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
