using MemoryEventBus.Domain.Events.Concrete;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using MemoryEventBus.Domain.Events.Interfaces.Producers;
using Microsoft.Extensions.Logging;

namespace MemoryEventBus.Infrastructure.Events.Producers.V1
{
    public class OrderPaidProducer : IOrderPaidProducer
    {
        private readonly IEventChannelManager _channelManager;
        private readonly ILogger<OrderPaidProducer> _logger;

        public OrderPaidProducer(IEventChannelManager channelManager, ILogger<OrderPaidProducer> logger)
        {
            _channelManager = channelManager;
            _logger = logger;
        }

        public async Task PublishAsync(OrderPaidEvent @event)
        {
            try
            {
                var success = await _channelManager.TryWriteAsync(@event);
                
                if (success)
                    _logger.LogInformation("Sent message for OrderPaidTopic.");
                else
                    _logger.LogInformation($"Failed to publish OrderPaidEvent {@event.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error publishing OrderPaidEvent {@event.EventId}: {ex.Message}", exception:ex);
                throw;
            }
        }
    }
}