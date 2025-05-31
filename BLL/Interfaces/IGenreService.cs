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
        Task<IEnumerable<GenreDto>> ReadAllAsync();
        Task<GenreDto> ReadByIdAsync(int id);
        Task CreateAsync(GenreDto dto);
        Task UpdateAsync(GenreDto dto);
        Task DeleteAsync(int id);
    }
}
