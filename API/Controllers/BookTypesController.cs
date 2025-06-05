using API.Models;
using AutoMapper;
using BLL.Facade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/booktypes")]
    [Authorize]
    public class BookTypesController : ControllerBase
    {
        private readonly ILibraryFacade _facade;
        private readonly IMapper _mapper;

        public BookTypesController(ILibraryFacade facade, IMapper mapper)
        {
            _facade = facade;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ReadAll()
        {
            var typeDtos = await _facade.ReadAllBookTypesAsync();
            var result = _mapper.Map<IEnumerable<BookTypeViewModel>>(typeDtos);
            return Ok(result);
        }
    }
}
