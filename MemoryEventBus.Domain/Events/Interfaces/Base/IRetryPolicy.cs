namespace MemoryEventBus.Domain.Events.Interfaces.Base
{
    /// <summary>
    /// Defines retry behavior for event processing including whether to retry and delay between attempts.
    /// </summary>
    public interface IRetryPolicy
    {
        /// <summary>
        /// Calculates the delay before the next retry attempt.
        /// </summary>
        /// <param name="attemptNumber">The current attempt number starting at1.</param>
        /// <returns>The delay to wait before retrying.</returns>
        TimeSpan GetDelay(int attemptNumber);

        /// <summary>
        /// Determines whether a retry should occur for the given attempt and exception.
        /// </summary>
        /// <param name="attemptNumber">The current attempt number.</param>
        /// <param name="exception">The exception thrown during processing.</param>
        /// <returns>True to retry; false to stop retrying.</returns>
        bool ShouldRetry(int attemptNumber, Exception exception);
    }
}