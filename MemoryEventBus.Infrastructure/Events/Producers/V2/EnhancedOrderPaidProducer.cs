using MemoryEventBus.Domain.Events.Concrete;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using MemoryEventBus.Domain.Events.Interfaces.Producers;
using Microsoft.Extensions.Logging;

namespace MemoryEventBus.Infrastructure.Events.Producers.V2
{
    public class EnhancedOrderPaidProducer : IOrderPaidProducer
    {
        private readonly IEventChannelManager _channelManager;
        private readonly ILogger<EnhancedOrderPaidProducer> _logger;
        private readonly IEventBusMetrics? _metrics;
        private readonly IRetryPolicy? _retryPolicy;

        public EnhancedOrderPaidProducer(
            IEventChannelManager channelManager, 
            ILogger<EnhancedOrderPaidProducer> logger,
            IEventBusMetrics? metrics = null,
            IRetryPolicy? retryPolicy = null)
        {
            _channelManager = channelManager;
            _logger = logger;
            _metrics = metrics;
            _retryPolicy = retryPolicy;
        }

        public async Task PublishAsync(OrderPaidEvent @event)
        {
            await PublishWithRetryAsync(@event);
        }

        private async Task PublishWithRetryAsync(OrderPaidEvent @event)
        {
            var attemptNumber = 1;

            while (true)
            {
                try
                {
                    var success = await _channelManager.TryWriteAsync(@event);
                    
                    if (success)
                    {
                        _logger.LogDebug("Successfully published OrderPaidEvent {EventId}", @event.EventId);
                        _metrics?.RecordEventPublished<OrderPaidEvent>(typeof(OrderPaidEvent).Name);
                        _logger.LogInformation("Sent message for OrderPaidTopic.");
                        return;
                    }

                    throw new InvalidOperationException("Channel write failed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error publishing OrderPaidEvent {EventId} on attempt {AttemptNumber}", 
                        @event.EventId, attemptNumber);

                    if (_retryPolicy == null || !_retryPolicy.ShouldRetry(attemptNumber, ex))
                    {
                        _logger.LogError(ex, "Failed to publish OrderPaidEvent {EventId} after {AttemptNumber} attempts", 
                            @event.EventId, attemptNumber);
                        _metrics?.RecordEventFailed<OrderPaidEvent>(typeof(OrderPaidEvent).Name, ex.GetType().Name);
                        _logger.LogError(ex, $"Failed to publish OrderPaidEvent {@event.EventId}");
                        throw;
                    }

                    _metrics?.RecordRetryAttempt<OrderPaidEvent>(typeof(OrderPaidEvent).Name, attemptNumber);
                    
                    var delay = _retryPolicy.GetDelay(attemptNumber);
                    _logger.LogInformation("Retrying OrderPaidEvent {EventId} in {DelayMs}ms (attempt {AttemptNumber})", 
                        @event.EventId, delay.TotalMilliseconds, attemptNumber + 1);

                    await Task.Delay(delay);
                    attemptNumber++;
                }
            }
        }
    }
}