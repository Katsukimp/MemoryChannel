using MemoryEventBus.Domain.Events.Aggregate;
using MemoryEventBus.Domain.Events.Concrete;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Channels;

namespace MemoryEventBus.Infrastructure.Events.Consumers
{
    public class EnhancedStockReducerConsumer : BackgroundService
    {
        private readonly Channel<DomainEvent> _channel;
        private readonly ILogger<EnhancedStockReducerConsumer> _logger;
        private readonly IEventBusErrorHandler? _errorHandler;
        private readonly IRetryPolicy? _retryPolicy;
        private readonly IEventBusMetrics? _metrics;

        public EnhancedStockReducerConsumer(
            IEventChannelManager channelManager,
            ILogger<EnhancedStockReducerConsumer> logger,
            IEventBusErrorHandler? errorHandler = null,
            IRetryPolicy? retryPolicy = null,
            IEventBusMetrics? metrics = null)
        {
            _channel = channelManager.GetOrCreateChannel<OrderPaidEvent>();
            _logger = logger;
            _errorHandler = errorHandler;
            _retryPolicy = retryPolicy;
            _metrics = metrics;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting enhanced consumer for OrderPaidEvent");

            await foreach (var domainEvent in _channel.Reader.ReadAllAsync(stoppingToken))
                if (domainEvent is OrderPaidEvent orderPaidEvent)
                    await ProcessWithRetryAsync(orderPaidEvent, stoppingToken);
        }

        private async Task ProcessWithRetryAsync(OrderPaidEvent @event, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var attemptNumber = 1;

            while (true)
            {
                try
                {
                    await ProcessEventAsync(@event, cancellationToken);
                    stopwatch.Stop();
                    _metrics?.RecordEventConsumed<OrderPaidEvent>(typeof(OrderPaidEvent).Name, stopwatch.Elapsed);
                    _logger.LogDebug("Successfully processed event {EventId} in {ElapsedMs}ms", 
                        @event.EventId, stopwatch.ElapsedMilliseconds);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing event {EventId} on attempt {AttemptNumber}", 
                        @event.EventId, attemptNumber);

                    if (_errorHandler != null)
                        await _errorHandler.HandleErrorAsync(@event, ex, attemptNumber, cancellationToken);

                    if (_retryPolicy == null || !_retryPolicy.ShouldRetry(attemptNumber, ex))
                    {
                        _logger.LogError("Failed to process event {EventId} after {AttemptNumber} attempts", 
                            @event.EventId, attemptNumber);
                        _metrics?.RecordEventFailed<OrderPaidEvent>(typeof(OrderPaidEvent).Name, ex.GetType().Name);
                        return;
                    }

                    _metrics?.RecordRetryAttempt<OrderPaidEvent>(typeof(OrderPaidEvent).Name, attemptNumber);
                    var delay = _retryPolicy.GetDelay(attemptNumber);
                    _logger.LogInformation("Retrying event {EventId} in {DelayMs}ms", @event.EventId, delay.TotalMilliseconds);

                    await Task.Delay(delay, cancellationToken);
                    attemptNumber++;
                }
            }
        }

        private async Task ProcessEventAsync(OrderPaidEvent @event, CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);
            
            if (Random.Shared.Next(1, 10) <= 2)
            {
                throw new InvalidOperationException("Simulated processing error for testing");
            }

            _logger.LogInformation($"Successfully processed OrderPaidEvent: {@event.EventName} | {@event.EventId} | Order Amount: {@event.Message.TotalAmount}");
        }
    }
}