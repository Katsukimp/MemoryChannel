using MemoryEventBus.Domain.Events.Aggregate;
using MemoryEventBus.Domain.Events.Concrete;
using MemoryEventBus.Domain.Events.Interfaces.Base;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace MemoryEventBus.Infrastructure.Events.Consumers
{
    public class StockReducerConsumer : BackgroundService
    {
        private readonly Channel<DomainEvent> _channel;
        private readonly ILogger<StockReducerConsumer> _logger;

        public StockReducerConsumer(IEventChannelManager channelManager, ILogger<StockReducerConsumer> logger)
        {
            _channel = channelManager.GetOrCreateChannel<OrderPaidEvent>();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var domainEvent in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                if (domainEvent is OrderPaidEvent orderPaidEvent)
                {
                    await ProcessEventAsync(orderPaidEvent, stoppingToken);
                }
            }
        }

        private async Task ProcessEventAsync(OrderPaidEvent @event, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(100, cancellationToken);
                
                _logger.LogInformation($"Successfully processed OrderPaidEvent: {@event.EventName} | {@event.EventId} | Order Amount: {@event.Message.TotalAmount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error processing OrderPaidEvent {@event.EventId}: {ex.Message}", exception:ex);
            }
        }
    }
}
