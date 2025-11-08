using MemoryEventBus.Domain.Events.Aggregate;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Channels;
using MemoryEventBus.Infrastructure.Events.Diagnostics;

namespace MemoryEventBus.Infrastructure.Events.Consumers.Base
{
    /// <summary>
    /// Base background service that consumes events of type <typeparamref name="TEvent"/> from an in-memory channel,
    /// applying retry and error handling policies while recording metrics and optional tracing.
    /// </summary>
    /// <typeparam name="TEvent">Concrete domain event type to consume.</typeparam>
    public abstract class BaseEventConsumer<TEvent> : BackgroundService where TEvent : DomainEvent
    {
        /// <summary>Underlying channel used to read events.</summary>
        protected readonly Channel<DomainEvent> _channel;
        /// <summary>Logger for diagnostic output.</summary>
        protected readonly ILogger<BaseEventConsumer<TEvent>> _logger;
        /// <summary>Error handler invoked on processing failures.</summary>
        protected readonly IEventBusErrorHandler _errorHandler;
        /// <summary>Retry policy controlling backoff and retry allowance.</summary>
        protected readonly IRetryPolicy _retryPolicy;
        /// <summary>Optional metrics recorder.</summary>
        protected readonly IEventBusMetrics? _metrics;
        /// <summary>Optional activity source for tracing.</summary>
        protected readonly EventBusActivitySource? _activitySource;

        /// <summary>
        /// Initializes a new instance of the consumer.
        /// </summary>
        /// <param name="channelManager">Channel manager to obtain the event channel.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="errorHandler">Error handler for failed attempts.</param>
        /// <param name="retryPolicy">Retry policy for transient failures.</param>
        /// <param name="metrics">Optional metrics implementation.</param>
        /// <param name="activitySource">Optional activity source for tracing.</param>
        protected BaseEventConsumer(
            IEventChannelManager channelManager,
            ILogger<BaseEventConsumer<TEvent>> logger,
            IEventBusErrorHandler errorHandler,
            IRetryPolicy retryPolicy,
            IEventBusMetrics? metrics = null,
            EventBusActivitySource? activitySource = null)
        {
            _channel = channelManager.GetOrCreateChannel<TEvent>();
            _logger = logger;
            _errorHandler = errorHandler;
            _retryPolicy = retryPolicy;
            _metrics = metrics;
            _activitySource = activitySource;
        }

        /// <summary>
        /// Main execution loop that continuously reads and processes events until cancellation is requested.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token signaled on host shutdown.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting consumer for event type {EventType}", typeof(TEvent).Name);

            await foreach (var domainEvent in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                if (domainEvent is TEvent typedEvent)
                {
                    await ProcessWithRetryAsync(typedEvent, stoppingToken);
                }
            }
        }

        private async Task ProcessWithRetryAsync(TEvent @event, CancellationToken cancellationToken)
        {
            using var activity = _activitySource?.StartConsumeActivity(typeof(TEvent).Name);
            var stopwatch = Stopwatch.StartNew();
            var attemptNumber = 1;

            while (true)
            {
                try
                {
                    await ProcessEventAsync(@event, cancellationToken);
                    stopwatch.Stop();
                    _metrics?.RecordEventConsumed<TEvent>(typeof(TEvent).Name, stopwatch.Elapsed);
                    activity?.SetTag("eventbus.consume.success", true);
                    _logger.LogDebug("Successfully processed event {EventId} of type {EventType} in {ElapsedMs}ms",
                        @event.EventId, typeof(TEvent).Name, stopwatch.ElapsedMilliseconds);
                    return;
                }
                catch (Exception ex)
                {
                    activity?.SetTag("eventbus.consume.error", ex.GetType().Name);
                    _logger.LogWarning(ex, "Error processing event {EventId} of type {EventType} on attempt {AttemptNumber}",
                        @event.EventId, typeof(TEvent).Name, attemptNumber);

                    await _errorHandler.HandleErrorAsync(@event, ex, attemptNumber, cancellationToken);

                    if (!_retryPolicy.ShouldRetry(attemptNumber, ex))
                    {
                        _logger.LogError("Failed to process event {EventId} of type {EventType} after {AttemptNumber} attempts. Giving up.",
                            @event.EventId, typeof(TEvent).Name, attemptNumber);
                        _metrics?.RecordEventFailed<TEvent>(typeof(TEvent).Name, ex.GetType().Name);
                        return;
                    }

                    _metrics?.RecordRetryAttempt<TEvent>(typeof(TEvent).Name, attemptNumber);
                    var delay = _retryPolicy.GetDelay(attemptNumber);
                    _logger.LogInformation("Retrying event {EventId} in {DelayMs}ms (attempt {AttemptNumber})",
                        @event.EventId, delay.TotalMilliseconds, attemptNumber + 1);

                    await Task.Delay(delay, cancellationToken);
                    attemptNumber++;
                }
            }
        }

        /// <summary>
        /// Processes a single event instance. Implementations should throw to trigger retries if desired.
        /// </summary>
        /// <param name="event">The event being processed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        protected abstract Task ProcessEventAsync(TEvent @event, CancellationToken cancellationToken);
    }
}
