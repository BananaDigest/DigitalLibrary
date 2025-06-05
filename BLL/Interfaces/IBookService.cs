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
        Task CreateAsync(ActionBookDto dto);
        Task DeleteAsync(int id);
        Task UpdateAsync(int bookId, ActionBookDto dto);
        Task<List<BookDto>> ReadByTypeAsync(int typeId);
    }
}
