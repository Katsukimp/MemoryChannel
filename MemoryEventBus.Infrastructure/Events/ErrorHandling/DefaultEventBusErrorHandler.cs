using MemoryEventBus.Domain.Events.Aggregate;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using Microsoft.Extensions.Logging;

namespace MemoryEventBus.Infrastructure.Events.ErrorHandling
{
    /// <summary>
    /// Default error handler that logs failures and consults per-event retry attempt limits.
    /// </summary>
    public class DefaultEventBusErrorHandler : IEventBusErrorHandler
    {
        private readonly ILogger<DefaultEventBusErrorHandler> _logger;
        private readonly Dictionary<Type, int> _maxRetryAttempts;

        /// <summary>
        /// Creates a new <see cref="DefaultEventBusErrorHandler"/>.
        /// </summary>
        public DefaultEventBusErrorHandler(ILogger<DefaultEventBusErrorHandler> logger)
        {
            _logger = logger;
            _maxRetryAttempts = new Dictionary<Type, int>
            {
                { typeof(DomainEvent), 3 }
            };
        }

        /// <inheritdoc />
        public async Task HandleErrorAsync<TEvent>(TEvent @event, Exception exception, int attemptNumber, CancellationToken cancellationToken = default)
            where TEvent : DomainEvent
        {
            _logger.LogError(exception,
                "Error processing event {EventType} with ID {EventId} on attempt {AttemptNumber}. Error: {ErrorMessage}",
                typeof(TEvent).Name, @event.EventId, attemptNumber, exception.Message);

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public bool ShouldRetry<TEvent>(TEvent @event, Exception exception, int attemptNumber)
            where TEvent : DomainEvent
        {
            var maxAttempts = _maxRetryAttempts.GetValueOrDefault(typeof(TEvent), 3);
            
            if (exception is ArgumentException or InvalidOperationException)
                return false;

            return attemptNumber < maxAttempts;
        }

        /// <summary>
        /// Sets the maximum number of retry attempts for a specific event type.
        /// </summary>
        public void SetMaxRetryAttempts<TEvent>(int maxAttempts) where TEvent : DomainEvent
        {
            _maxRetryAttempts[typeof(TEvent)] = maxAttempts;
        }
    }
}