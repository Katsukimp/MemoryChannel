using MemoryEventBus.Domain.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace MemoryEventBus.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderController : ControllerBase
    {

        private readonly IPayUseCase _payUseCase;
        public OrderController(IPayUseCase payUseCase)
        {
            _payUseCase = payUseCase;
        }

        [HttpPost("process-payment")]
        public async Task<IActionResult> ProcessPaymentAsync([FromQuery] decimal amount)
        {
            var order = await _payUseCase.ExecuteAsync(amount);
            return order is null ? BadRequest("Payment processing failed.") : Ok(order);
        }
    }
}
