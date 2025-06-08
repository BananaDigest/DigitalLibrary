using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public ImageUploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Завантажити зображення книги у папку <ProjectRoot>/image/books.
        /// </summary>
        [HttpPost("books")]
        [AllowAnonymous] // Можна обмежити доступ, якщо потрібно
        public async Task<IActionResult> UploadBookImage(IFormFile file)
        {
            if (file == null)
                return BadRequest(new { message = "Файл не передано." });

            // Дозволені розширення
            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || Array.IndexOf(permittedExtensions, extension) < 0)
            {
                return BadRequest(new { message = "Непідтримуване розширення файлу." });
            }

            // ContentRootPath → корінь проєкту (де лежить .csproj)
            var projectRoot = _env.ContentRootPath;
            // Папка image/books у корені
            var targetFolder = Path.Combine(projectRoot, "image", "books");

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            // Унікальне ім'я файлу
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(targetFolder, uniqueFileName);

            try
            {
                await using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Помилка збереження файлу", detail = ex.Message });
            }

            // Якщо потім потрібно віддавати файл як статичний, можна сформувати URL.
            // Наприклад, якщо ми додатково налаштуємо StaticFiles так, щоб сервити /image.
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/image/books/{uniqueFileName}";

            return Ok(new
            {
                message = "Файл успішно завантажено.",
                fileName = uniqueFileName,
                url = fileUrl
            });
        }
    }
}
