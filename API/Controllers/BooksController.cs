using AutoMapper;
using BLL.DTOs;
using BLL.Facade;
using BLL.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly LibraryFacade _facade;
        public BooksController(LibraryFacade facade) => _facade = facade;

        // POST api/book
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateBook([FromBody] ActionBookDto dto)
        {
            var id = await _facade.CreateBookAsync(dto);
            return CreatedAtAction(nameof(ReadBook), new { id }, id);
        }

        // GET api/book
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActionBookDto>>> ReadBooks([FromQuery] string filterBy, [FromQuery] string term)
        {
            var dtos = await _facade.FindBooksAsync(term, filterBy);
            return Ok(dtos);
        }

        // GET api/book/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<BookCopy>>> ReadBook(Guid id)
        {
            var copies = await _facade.ReadAvailableCopiesAsync(id);
            return Ok(copies);
        }

        // DELETE api/book/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(Guid id)
        {
            await _facade.DeleteBookAsync(id);
            return NoContent();
        }
    }

}
