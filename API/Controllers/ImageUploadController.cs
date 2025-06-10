using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.RegularExpressions;
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

        [HttpPost("books")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadBookImage(
            IFormFile file,
            [FromForm] string bookName)
        {
            if (file == null)
                return BadRequest(new { message = "Файл не передано." });

            if (string.IsNullOrWhiteSpace(bookName))
                return BadRequest(new { message = "Назва книги не вказана." });

            // Дозволені розширення
            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || Array.IndexOf(permittedExtensions, extension) < 0)
                return BadRequest(new { message = "Непідтримуване розширення файлу." });

            var safeBookName = Regex.Replace(bookName.Trim(), @"[^\w\-]", "_");

            // Кінцева папка: <ProjectRoot>/image/books
            var projectRoot = _env.ContentRootPath;
            var targetFolder = Path.Combine(projectRoot, "image", "books");
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            // Ім’я файлу – за назвою книги
            var fileName = Path.GetFileName(file.FileName);
            var fullPath = Path.Combine(targetFolder, fileName);

            try
            {
                // Перезаписуємо, якщо файл існує
                await using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Помилка збереження файлу", detail = ex.Message });
            }

            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var fileUrl = $"{baseUrl}/image/books/{fileName}";

            return Ok(new
            {
                message = "Файл успішно завантажено.",
                bookName = bookName,
                fileName = fileName,
                url = fileUrl
            });
        }
    }
}
