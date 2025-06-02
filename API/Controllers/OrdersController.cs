using API.Models;
using AutoMapper;
using BLL.DTOs;
using BLL.Facade;
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
        private readonly ILibraryFacade _facade;
        private readonly IMapper _mapper;

        public OrdersController(ILibraryFacade facade, IMapper mapper)
        {
            _facade = facade;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Manager,Administrator")]
        public async Task<IActionResult> GetAll()
        {
            var orderDtos = await _facade.ReadAllOrdersAsync();
            var result = _mapper.Map<IEnumerable<OrderViewModel>>(orderDtos);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var orderDto = await _facade.ReadOrderByIdAsync(id);
            if (orderDto == null)
                return NotFound();

            var result = _mapper.Map<OrderViewModel>(orderDto);
            return Ok(result);
        }

        [HttpGet("by-user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var orderDtos = await _facade.ReadOrdersByUserAsync(userId);
            var result = _mapper.Map<IEnumerable<OrderViewModel>>(orderDtos);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = _mapper.Map<ActionOrderDto>(model);
            await _facade.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, null);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _facade.DeleteOrderAsync(id);
            return NoContent();
        }
    }
}
