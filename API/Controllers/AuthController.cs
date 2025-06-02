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

            return Ok(new { message = "Успішний вхід." });
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
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, null);
        }

        // ==================== Отримати дані про користувача ====================
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Manager,Administrator")]
        public async Task<IActionResult> GetById(int id)
        {
            var userDto = await _facade.GetUserByIdAsync(id);
            if (userDto == null)
                return NotFound();

            var result = _mapper.Map<UserViewModel>(userDto);
            return Ok(result);
        }

        // ==================== Оновлення даних користувача ====================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Manager,Administrator")]
        public async Task<IActionResult> Update(int id, [FromBody] UserActionModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = _mapper.Map<UserDto>(model);
            dto.Id = id;
            await _facade.UpdateUserAsync(dto);
            return NoContent();
        }

        // ==================== Видалення користувача ====================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
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
