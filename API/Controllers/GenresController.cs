using API.Models;
using AutoMapper;
using BLL.DTOs;
using BLL.Facade;
using BLL.Interfaces;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/genres")]
    public class GenresController : ControllerBase
    {
        private readonly ILibraryFacade _facade;
        private readonly IMapper _mapper;
        private readonly IBookService _bookService;


        public GenresController(ILibraryFacade facade, IMapper mapper, IBookService bookService)
        {
            _facade = facade;
            _mapper = mapper;
            _bookService = bookService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ReadAll()
        {
            var genreDtos = await _facade.ReadAllGenresAsync();
            var result = _mapper.Map<IEnumerable<GenreViewModel>>(genreDtos);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ReadById(int id)
        {
            var genreDto = await _facade.ReadGenreByIdAsync(id);
            if (genreDto == null)
                return NotFound();

            var result = _mapper.Map<GenreViewModel>(genreDto);
            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] GenreViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = _mapper.Map<GenreDto>(model);
            await _facade.CreateGenreAsync(dto);
            return CreatedAtAction(nameof(ReadById), new { id = dto.Id }, null);
        }

        [HttpPut("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Update(int id, [FromBody] GenreViewModel model)
        {
            if (!ModelState.IsValid || id != model.Id)
                return BadRequest();

            var dto = _mapper.Map<GenreDto>(model);
            await _facade.UpdateGenreAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Delete(int id)
        {
            var allBooks = await _bookService.ReadAllAsync();

            bool genreInUse = allBooks.Any(book => book.GenreId == id);

            if (genreInUse)
            {
                return BadRequest("Жанр не можна видалити, оскільки існують книги з цим жанром.");
            }

            await _facade.DeleteGenreAsync(id);
            return NoContent();
        }
    }
}
