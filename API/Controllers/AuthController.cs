using API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using BLL.DTOs;
using BLL.Facade;
using Microsoft.AspNetCore.Authorization;
using API.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILibraryFacade _facade;
        private readonly IMapper _mapper;

        public AuthController(ILibraryFacade facade, IMapper mapper)
        {
            _facade = facade;
            _mapper = mapper;
        }

        // ==================== Login ====================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userDto = await _facade.AuthenticateAsync(model.Username, model.Password);
            if (userDto == null)
                return Unauthorized(new { message = "Невірний email або пароль." });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                new Claim(ClaimTypes.Name, userDto.Email),
                new Claim(ClaimTypes.Role, userDto.Role) // Registered, Manager чи Administrator
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = System.DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Ok(new { message = "Успішний вхід.", token = "qwerty" });
        }

        // ==================== Logout ====================
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Успішний вихід." });
        }

        // ==================== Реєстрація ====================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserActionModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = _mapper.Map<UserDto>(model);
            dto.Role = "Registered";

            var created = await _facade.RegisterUserAsync(dto);
            return CreatedAtAction(nameof(ReadById), new { id = created.Id }, null);
        }

        // ==================== Отримати дані про користувача ====================
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> ReadById(int id)
        {
            var userDto = await _facade.ReadUserByIdAsync(id);
            if (userDto == null)
                return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentRole != "Administrator" && currentRole != "Manager" && currentUserId != id)
                return Forbid(); // 403

            var result = _mapper.Map<UserViewModel>(userDto);
            return Ok(result);
        }

        //[HttpGet("get-all-users")]
        //[Authorize(Roles = "Manager,Administrator, Registered")]
        //public async Task<IActionResult> ReadAll()
        //{
        //    var users = await _facade.ReadAllUsersAsync();
        //    // Передамо у ViewModel (якщо потрібна додаткова фільтрація) або просто повернемо як є:
        //    return Ok(users);
        //}
        [HttpGet("get-all-users")]
        public async Task<IActionResult> ReadAll()
        {
            // Перевірка, чи користувач аутентифікований і має одну з потрібних ролей
            if (User.Identity.IsAuthenticated)
            {
                var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
                if (roles.Contains("Manager") || roles.Contains("Administrator") || roles.Contains("Registered"))
                {
                    var users = await _facade.ReadAllUsersAsync();
                    return Ok(users);
                }
                else
                {
                    return Forbid();
                }
            }

            // Якщо користувач не аутентифікований, перевіряємо Bearer токен
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

            var usersByToken = await _facade.ReadAllUsersAsync();
            return Ok(usersByToken);
        }


        // ==================== Оновлення даних користувача ====================
        [HttpPut("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Update(int id, [FromBody] UserActionModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentRole != "Administrator" && currentRole != "Manager" && currentUserId != id)
                return Forbid(); // 403

            var dto = _mapper.Map<UserDto>(model);
            dto.Id = id;
            await _facade.UpdateUserAsync(dto);
            return NoContent();
        }

        // ==================== Видалення користувача ====================
        [HttpDelete("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentRole != "Administrator" && currentUserId != id)
                return Forbid(); // 403
            await _facade.DeleteUserAsync(id);
            return NoContent();
        }

        // ==================== Access Denied ====================
        [HttpGet("denied")]
        public IActionResult AccessDenied()
        {
            return Forbid();
        }
    }
}
