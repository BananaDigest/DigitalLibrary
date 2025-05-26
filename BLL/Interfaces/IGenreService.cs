using BLL.DTOs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IGenreService
    {
        Task<IEnumerable<GenreDto>> GetAllAsync();
        Task<GenreDto> GetByIdAsync(Guid id);
        Task CreateAsync(GenreDto dto);
        Task UpdateAsync(GenreDto dto);
        Task DeleteAsync(Guid id);
    }
}
