using API.Models;
using AutoMapper;
using BLL.DTOs;
using BLL.Facade;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly ILibraryFacade _facade;
        private readonly IMapper _mapper;

        public BooksController(ILibraryFacade facade, IMapper mapper)
        {
            _facade = facade;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous] // усі можуть дивитися каталог без логіну
        public async Task<IActionResult> ReadAll()
        {
            var bookDtos = await _facade.ReadAllBooksAsync();
            var result = _mapper.Map<IEnumerable<BookViewModel>>(bookDtos);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create([FromBody] CreateBookModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = _mapper.Map<ActionBookDto>(model);
            await _facade.CreateBookAsync(dto);
            return CreatedAtAction(nameof(ReadBookById), new { id = dto.Id }, null);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateBookModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = _mapper.Map<ActionBookDto>(model);
            dto.Id = id;
            await _facade.UpdateBookAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            await _facade.DeleteBookAsync(id);
            return NoContent();
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ReadBookById(int id)
        {
            var bookDto = await _facade.ReadBookByIdAsync(id);
            if (bookDto == null)
                return NotFound();

            var result = _mapper.Map<BookViewModel>(bookDto);
            return Ok(result);
        }
    }
}
