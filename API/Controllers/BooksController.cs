using AutoMapper;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _svc;

        public BooksController(IBookService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _svc.ReadAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => Ok(await _svc.ReadByIdAsync(id));

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term)
            => Ok(await _svc.SearchAsync(term));

        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Create([FromBody] ActionBookDto dto)
        {
            await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, null);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ActionBookDto dto)
        {
            dto.Id = id;
            await _svc.UpdateAsync(dto.Id, dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.DeleteAsync(id);
            return NoContent();
        }
    }
}
