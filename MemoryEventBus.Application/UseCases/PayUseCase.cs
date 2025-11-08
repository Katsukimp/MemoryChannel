using MemoryEventBus.Domain.Entities;
using MemoryEventBus.Domain.Events.Concrete;
using MemoryEventBus.Domain.Events.Interfaces.Producers;
using MemoryEventBus.Domain.UseCases;
using Microsoft.Extensions.Logging;

namespace MemoryEventBus.Application.UseCases
{
    public class PayUseCase : IPayUseCase
    {
        private readonly IOrderPaidProducer _orderPaidProducer;
        private readonly ILogger<PayUseCase> _logger;
        public PayUseCase(IOrderPaidProducer orderPaidProducer, ILogger<PayUseCase> logger)
        {
            _orderPaidProducer = orderPaidProducer;
            _logger = logger;
        }

        public async Task<Order?> ExecuteAsync(decimal amount)
        {
            try
            {
                var order = new Order
                {
                    TotalAmount = 100.00m
                };

                _logger.LogInformation($"Processing payment of amount: {amount} - Id: {order.Id}");
                order.Pay(amount);

                var publishTasks = order.GetDomainEvents()
                    .OfType<OrderPaidEvent>()
                    .Select(domainEvent => _orderPaidProducer.PublishAsync(domainEvent));

                await Task.WhenAll(publishTasks);
                order.ClearEvents();

                return order;
            }
            catch (AggregateException exception)
            {
                _logger.LogError(exception, $"Failed to pay order - {exception.Message}");
                return null;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error in process - {exception.Message}");
                return null;
            }
        }
    }
}
