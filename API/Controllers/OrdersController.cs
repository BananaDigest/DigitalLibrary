using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _svc;

        public OrdersController(IOrderService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _svc.GetAllAsync());

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(await _svc.GetByIdAsync(id));

        [HttpGet("by-user/{userId:guid}")]
        public async Task<IActionResult> GetByUser(Guid userId)
            => Ok(await _svc.GetByUserAsync(userId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ActionOrderDto dto)
        {
            await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, null);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ActionOrderDto dto)
        {
            dto.Id = id;
            await _svc.UpdateAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _svc.DeleteAsync(id);
            return NoContent();
        }
    }
}
