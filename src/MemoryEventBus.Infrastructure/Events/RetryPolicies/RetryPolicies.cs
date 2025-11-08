using MemoryEventBus.Domain.Events.Interfaces.Base;

namespace MemoryEventBus.Infrastructure.Events.RetryPolicies
{
    /// <summary>
    /// Retry policy implementing exponential backoff up to a maximum delay and attempt count.
    /// </summary>
    public class ExponentialBackoffRetryPolicy : IRetryPolicy
    {
        private readonly int _maxAttempts;
        private readonly TimeSpan _baseDelay;
        private readonly TimeSpan _maxDelay;

        /// <summary>
        /// Creates a new <see cref="ExponentialBackoffRetryPolicy"/>.
        /// </summary>
        public ExponentialBackoffRetryPolicy(int maxAttempts = 3, TimeSpan? baseDelay = null, TimeSpan? maxDelay = null)
        {
            _maxAttempts = maxAttempts;
            _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(100);
            _maxDelay = maxDelay ?? TimeSpan.FromSeconds(30);
        }

        /// <inheritdoc />
        public TimeSpan GetDelay(int attemptNumber)
        {
            var delay = TimeSpan.FromMilliseconds(_baseDelay.TotalMilliseconds * Math.Pow(2, attemptNumber - 1));
            return delay > _maxDelay ? _maxDelay : delay;
        }

        /// <inheritdoc />
        public bool ShouldRetry(int attemptNumber, Exception exception)
        {
            if (exception is ArgumentException or ArgumentNullException or InvalidOperationException)
                return false;

            return attemptNumber <= _maxAttempts;
        }
    }

    /// <summary>
    /// Retry policy using a constant linear delay between attempts.
    /// </summary>
    public class LinearRetryPolicy : IRetryPolicy
    {
        private readonly int _maxAttempts;
        private readonly TimeSpan _delay;

        /// <summary>
        /// Creates a new <see cref="LinearRetryPolicy"/>.
        /// </summary>
        public LinearRetryPolicy(int maxAttempts = 3, TimeSpan? delay = null)
        {
            _maxAttempts = maxAttempts;
            _delay = delay ?? TimeSpan.FromMilliseconds(500);
        }

        /// <inheritdoc />
        public TimeSpan GetDelay(int attemptNumber) => _delay;

        /// <inheritdoc />
        public bool ShouldRetry(int attemptNumber, Exception exception)
        {
            if (exception is ArgumentException or ArgumentNullException or InvalidOperationException)
                return false;

            return attemptNumber <= _maxAttempts;
        }
    }
}