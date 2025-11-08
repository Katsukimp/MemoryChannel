using MemoryEventBus.Domain.Entities;

namespace MemoryEventBus.Domain.UseCases
{
    public interface IPayUseCase
    {
        Task<Order?> ExecuteAsync(decimal amount);
    }
}
