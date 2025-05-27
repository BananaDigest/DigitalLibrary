using BLL.DTOs;
using BLL.Facade;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly LibraryFacade _facade;
        public OrdersController(LibraryFacade facade) => _facade = facade;

        // POST api/order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] ActionOrderDto dto)
        {
            await _facade.CreateOrderAsync(dto);
            return Created(string.Empty, null);
        }

        // DELETE api/order/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            await _facade.DeleteOrderAsync(id);
            return NoContent();
        }
    }
}
