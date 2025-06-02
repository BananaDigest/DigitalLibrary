using API.Models;
using AutoMapper;
using BLL.DTOs;
using BLL.Facade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/genres")]
    [Authorize(Roles = "Manager,Administrator")]
    public class GenresController : ControllerBase
    {
        private readonly ILibraryFacade _facade;
        private readonly IMapper _mapper;

        public GenresController(ILibraryFacade facade, IMapper mapper)
        {
            _facade = facade;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var genreDtos = await _facade.GetAllGenresAsync();
            var result = _mapper.Map<IEnumerable<GenreViewModel>>(genreDtos);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var genreDto = await _facade.GetGenreByIdAsync(id);
            if (genreDto == null)
                return NotFound();

            var result = _mapper.Map<GenreViewModel>(genreDto);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GenreViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = _mapper.Map<GenreDto>(model);
            await _facade.CreateGenreAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, null);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] GenreViewModel model)
        {
            if (!ModelState.IsValid || id != model.Id)
                return BadRequest();

            var dto = _mapper.Map<GenreDto>(model);
            await _facade.UpdateGenreAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _facade.DeleteGenreAsync(id);
            return NoContent();
        }
    }
}
