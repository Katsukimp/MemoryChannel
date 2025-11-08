using MemoryEventBus.Domain.Events.Interfaces.Base;
using System.Diagnostics.Metrics;

namespace MemoryEventBus.Infrastructure.Events.Metrics
{
    public class EventBusMetrics : IEventBusMetrics, IDisposable
    {
        private readonly Meter _meter;
        private readonly Counter<long> _eventsPublishedCounter;
        private readonly Counter<long> _eventsConsumedCounter;
        private readonly Counter<long> _eventsFailedCounter;
        private readonly Counter<long> _retryAttemptsCounter;
        private readonly Histogram<double> _processingTimeHistogram;
        private readonly ObservableGauge<int> _channelDepthGauge;
        private readonly Dictionary<string, int> _channelDepths;

        public EventBusMetrics()
        {
            _meter = new Meter("MemoryEventBus", "1.0.0");
            _channelDepths = [];

            _eventsPublishedCounter = _meter.CreateCounter<long>(
                "eventbus_events_published_total",
                "events",
                "Total number of events published");

            _eventsConsumedCounter = _meter.CreateCounter<long>(
                "eventbus_events_consumed_total",
                "events",
                "Total number of events consumed successfully");

            _eventsFailedCounter = _meter.CreateCounter<long>(
                "eventbus_events_failed_total",
                "events",
                "Total number of events that failed processing");

            _retryAttemptsCounter = _meter.CreateCounter<long>(
                "eventbus_retry_attempts_total",
                "attempts",
                "Total number of retry attempts");

            _processingTimeHistogram = _meter.CreateHistogram<double>(
                "eventbus_event_processing_duration_seconds",
                "seconds",
                "Time taken to process events");

            _channelDepthGauge = _meter.CreateObservableGauge<int>(
                "eventbus_channel_depth",
                () => GetChannelDepthMeasurements(),
                "events",
                "Current depth of event channels");
        }

        private IEnumerable<Measurement<int>> GetChannelDepthMeasurements()
        {
            foreach (var kvp in _channelDepths)
            {
                yield return new Measurement<int>(kvp.Value, new KeyValuePair<string, object?>("channel_type", kvp.Key));
            }
        }

        public void RecordEventPublished<TEvent>(string eventType)
        {
            _eventsPublishedCounter.Add(1, new KeyValuePair<string, object?>("event_type", eventType));
        }

        public void RecordEventConsumed<TEvent>(string eventType, TimeSpan processingTime)
        {
            _eventsConsumedCounter.Add(1, new KeyValuePair<string, object?>("event_type", eventType));
            _processingTimeHistogram.Record(processingTime.TotalSeconds, new KeyValuePair<string, object?>("event_type", eventType));
        }

        public void RecordEventFailed<TEvent>(string eventType, string errorType)
        {
            _eventsFailedCounter.Add(1, 
                new KeyValuePair<string, object?>("event_type", eventType),
                new KeyValuePair<string, object?>("error_type", errorType));
        }

        public void RecordRetryAttempt<TEvent>(string eventType, int attemptNumber)
        {
            _retryAttemptsCounter.Add(1, 
                new KeyValuePair<string, object?>("event_type", eventType),
                new KeyValuePair<string, object?>("attempt_number", attemptNumber));
        }

        public void RecordChannelDepth(string channelType, int depth)
        {
            _channelDepths[channelType] = depth;
        }

        public void Dispose()
        {
            _meter?.Dispose();
        }
    }
}