using BLL.DTOs;
using Domain.Enums;
using System;
using System.Collections;

namespace BLL.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> ReadAllAsync();
        Task<BookDto> ReadByIdAsync(Guid id);
        Task<IEnumerable<BookDto>> SearchAsync(string term);
        Task<IEnumerable<BookDto>> FilterByTypeAsync(BookType type);
        Task<IEnumerable<BookDto>> FilterByGenreAsync(Guid genreId);
        Task CreateAsync(ActionBookDto dto);
        Task DeleteAsync(Guid id);
        Task UpdateAsync(ActionBookDto dto);
    }
}
