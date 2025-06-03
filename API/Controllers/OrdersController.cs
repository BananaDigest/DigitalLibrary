using API.Models;
using AutoMapper;
using BLL.DTOs;
using BLL.Facade;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        [Authorize(Roles = "Manager,Administrator,Registered")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentRole != "Administrator" && currentRole != "Manager" && currentUserId != id)
                return Forbid(); // 403

            var orderDto = await _facade.ReadOrderByIdAsync(id);
            if (orderDto == null)
                return NotFound();

            var result = _mapper.Map<OrderViewModel>(orderDto);
            return Ok(result);
        }

        [HttpGet("by-user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
        {

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentRole != "Administrator" && currentRole != "Manager" && currentUserId != userId)
                return Forbid(); // 403
            var orderDtos = await _facade.ReadOrdersByUserAsync(userId);
            var result = _mapper.Map<IEnumerable<OrderViewModel>>(orderDtos);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Беремо ID поточного користувача та роль із Claims
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // 2. Визначаємо, чи можемо використовувати model.UserId,
            //    аби потім заповнити dto.UserId.
            int finalUserId;
            if (currentRole == "Administrator" || currentRole == "Manager")
            {
                // Адмін і Менеджер можуть вказувати будь-який UserId у запиті:
                finalUserId = model.UserId;
            }
            else
            {
                // Усі інші (Registered і гість) можуть створювати замовлення
                // тільки для "себе" → використовуємо поточний ID
                finalUserId = currentUserId;
            }

            // 3. Мапимо решту полів у ActionOrderDto
            var dto = _mapper.Map<ActionOrderDto>(model);

            // 4. Перезаписуємо UserId з визначеного вище finalUserId
            dto.UserId = finalUserId;

            // 5. Створюємо замовлення
            await _facade.CreateOrderAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, null);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Отримуємо сам об’єкт замовлення (щоб дізнатися, кому воно належить)
            var orderDto = await _facade.ReadOrderByIdAsync(id);
            if (orderDto == null)
                return NotFound();

            // 2. Зчитуємо поточний UserId та роль із Claims
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // 3. Якщо не Administrator і замовлення не належить поточному користувачеві → заборонено
            if (currentRole != "Administrator" && orderDto.UserId != currentUserId)
                return Forbid();

            // 4. Інакше видаляємо
            await _facade.DeleteOrderAsync(id);
            return NoContent();
        }
    }
}
