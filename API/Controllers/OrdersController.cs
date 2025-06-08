using API.Models;
using AutoMapper;
using BLL.DTOs;
using BLL.Facade;
using BLL.Interfaces;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/orders")]
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

        public async Task<IActionResult> ReadAll()
        {
            var orderDtos = await _facade.ReadAllOrdersAsync();
            var result = _mapper.Map<IEnumerable<OrderViewModel>>(orderDtos);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> ReadById(int id)
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

        //[HttpGet("by-user/{userId:int}")]
        //public async Task<IActionResult> ReadByUser(int userId)
        //{

        //    var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //    var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;

        //    if (currentRole != "Administrator" && currentRole != "Manager" && currentUserId != userId)
        //        return Forbid(); // 403
        //    var orderDtos = await _facade.ReadOrdersByUserAsync(userId);
        //    var result = _mapper.Map<IEnumerable<OrderViewModel>>(orderDtos);
        //    return Ok(result);
        //}


        [HttpGet("by-user/{userId:int}")]
        public async Task<IActionResult> ReadByUser(int userId)
        {
            // Якщо користувач автентифікований (JWT токен)
            if (User.Identity.IsAuthenticated)
            {
                var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    if (roles.Contains("Administrator") || roles.Contains("Manager") || currentUserId == userId)
                    {
                        var orderDtos = await _facade.ReadOrdersByUserAsync(userId);
                        var result = _mapper.Map<IEnumerable<OrderViewModel>>(orderDtos);
                        return Ok(result);
                    }

                    return Forbid();
                }

                return Unauthorized(new { message = "Некоректні клейми в токені" });
            }

            // Якщо не автентифікований — перевірка статичного токена
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "Відсутній або неправильний токен авторизації" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            if (token != "qwerty")
            {
                return Unauthorized(new { message = "Невірний токен" });
            }

            // Якщо токен правильний — дозволяємо доступ
            var ordersByToken = await _facade.ReadOrdersByUserAsync(userId);
            var resultByToken = _mapper.Map<IEnumerable<OrderViewModel>>(ordersByToken);
            return Ok(resultByToken);
        }


        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] CreateOrderModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    // 1. Беремо ID поточного користувача та роль із Claims
        //    var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //    var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;

        //    // 2. Визначаємо, чи можемо використовувати model.UserId,
        //    //    аби потім заповнити dto.UserId.
        //    int finalUserId;
        //    if (currentRole == "Administrator" || currentRole == "Manager")
        //    {
        //        // Адмін і Менеджер можуть вказувати будь-який UserId у запиті:
        //        finalUserId = model.UserId;
        //    }
        //    else
        //    {
        //        // Усі інші (Registered і гість) можуть створювати замовлення
        //        // тільки для "себе" → використовуємо поточний ID
        //        finalUserId = currentUserId;
        //    }

        //    // 3. Мапимо решту полів у ActionOrderDto
        //    var dto = _mapper.Map<ActionOrderDto>(model);

        //    // 4. Перезаписуємо UserId з визначеного вище finalUserId
        //    dto.UserId = finalUserId;

        //    // 5. Створюємо замовлення
        //    await _facade.CreateOrderAsync(dto);

        //    return CreatedAtAction(nameof(ReadById), new { id = dto.Id }, null);
        //}


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int finalUserId;

            // Якщо користувач автентифікований через JWT
            if (User.Identity.IsAuthenticated)
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var currentRoleClaim = User.FindFirst(ClaimTypes.Role);

                if (currentUserIdClaim == null || currentRoleClaim == null)
                {
                    return Unauthorized(new { message = "Некоректні клейми в токені" });
                }

                int currentUserId = int.Parse(currentUserIdClaim.Value);
                var currentRole = currentRoleClaim.Value;

                // Якщо роль дозволяє вказати інший UserId — використовуємо його
                if (currentRole == "Administrator" || currentRole == "Manager")
                {
                    finalUserId = model.UserId;
                }
                else
                {
                    // Інакше використовуємо поточний ID користувача
                    finalUserId = currentUserId;
                }
            }
            else
            {
                // Якщо користувач не автентифікований — перевіряємо статичний токен
                var authHeader = Request.Headers["Authorization"].ToString();

                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { message = "Відсутній або неправильний токен авторизації" });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();

                if (token != "qwerty")
                {
                    return Unauthorized(new { message = "Невірний токен" });
                }

                // Якщо токен правильний — дозволяємо використати UserId з моделі
                finalUserId = model.UserId;
            }

            // Мапимо модель у DTO
            var dto = _mapper.Map<ActionOrderDto>(model);
            dto.UserId = finalUserId;

            await _facade.CreateOrderAsync(dto);

            return CreatedAtAction(nameof(ReadById), new { id = dto.Id }, null);
        }


        [HttpPatch("{id:int}/status")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            try
            {
                await _facade.UpdateStatusAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpDelete("{id:int}")]
        //[AllowAnonymous]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var isAdmin = User.IsInRole("Administrator");
        //    try
        //    {
        //        // 1. Отримуємо сам об’єкт замовлення (щоб дізнатися, кому воно належить)
        //        var orderDto = await _facade.ReadOrderByIdAsync(id);
        //    if (orderDto == null)
        //        return NotFound();

        //    // 2. Зчитуємо поточний UserId та роль із Claims
        //    var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //    var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;

        //    // 3. Якщо не Administrator і замовлення не належить поточному користувачеві → заборонено
        //    if (currentRole != "Administrator" && orderDto.UserId != currentUserId)
        //        return Forbid();

        //    // 4. Інакше видаляємо
        //    await _facade.DeleteOrderAsync(id, isAdmin);
        //    return NoContent();
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        return Forbid();
        //    }
        //    catch (KeyNotFoundException)
        //    {
        //        return NotFound();
        //    }
        //}


        [HttpDelete("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Delete(int id)
        {
            bool isAdmin = false;
            int? currentUserId = null;
            bool tokenIsStatic = false;

            // Якщо користувач автентифікований
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || roleClaim == null)
                    return Unauthorized(new { message = "Некоректні клейми в токені" });

                currentUserId = int.Parse(userIdClaim.Value);
                var currentRole = roleClaim.Value;
                isAdmin = currentRole == "Administrator";
            }
            else
            {
                // Bearer token
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                    return Unauthorized(new { message = "Відсутній або неправильний токен авторизації" });

                var token = authHeader.Substring("Bearer ".Length).Trim();
                if (token != "qwerty")
                    return Unauthorized(new { message = "Невірний токен" });

                tokenIsStatic = true;
            }

            try
            {
                var orderDto = await _facade.ReadOrderByIdAsync(id);
                if (orderDto == null)
                    return NotFound();

                // Доступ дозволено:
                // 1. Якщо користувач — адмін
                // 2. Якщо користувач — власник замовлення
                // 3. Якщо токен — статичний (qwerty)
                if (!(isAdmin || tokenIsStatic || (currentUserId.HasValue && orderDto.UserId == currentUserId.Value)))
                    return Forbid();

                await _facade.DeleteOrderAsync(id, isAdmin);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }


    }
}
