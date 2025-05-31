using BLL.DTOs;
using Domain.Enums;
using System;
using System.Collections;

namespace BLL.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> ReadAllAsync();
        Task<BookDto> ReadByIdAsync(int id);
        Task<IEnumerable<BookDto>> SearchAsync(string term);
        Task<IEnumerable<BookDto>> FilterByTypeAsync(int typeId);
        Task<IEnumerable<BookDto>> FilterByGenreAsync(int genreId);
        Task CreateAsync(ActionBookDto dto);
        Task DeleteAsync(int id);
        Task UpdateAsync(ActionBookDto dto);
    }
}
