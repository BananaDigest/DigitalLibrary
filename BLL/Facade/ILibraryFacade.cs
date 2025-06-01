using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Facade
{
    public interface ILibraryFacade
    {
        public Task<IEnumerable<BookDto>> GetAllBooksAsync();
        public Task<BookDto> GetBookByIdAsync(int id);
        public Task<IEnumerable<BookDto>> SearchBooksAsync(string term);
        public Task<IEnumerable<BookDto>> FilterBooksByTypeAsync(int typeId);
        public Task<IEnumerable<BookDto>> FilterBooksByGenreAsync(int genreId);
        public Task CreateBookAsync(ActionBookDto dto);
        public Task UpdateBookAsync(int bookId, ActionBookDto dto);
        public Task<List<BookDto>> ReadBooksByTypeAsync(int typeId);
        public Task DeleteBookAsync(int id);
        public Task<IEnumerable<GenreDto>> GetAllGenresAsync();
        public Task<GenreDto> GetGenreByIdAsync(int id);
        public Task CreateGenreAsync(GenreDto dto);
        public Task UpdateGenreAsync(GenreDto dto);
        public Task DeleteGenreAsync(int id);
        public Task<IEnumerable<OrderDto>> ReadAllOrdersAsync();
        public Task<OrderDto> ReadOrderByIdAsync(int id);
        public Task<IEnumerable<OrderDto>> ReadOrdersByUserAsync(int userId);
        public Task CreateOrderAsync(ActionOrderDto dto);
        public Task DeleteOrderAsync(int id);
        public Task<UserDto> RegisterUserAsync(UserDto dto);
        public Task<UserDto> GetUserByIdAsync(int id);
        public Task<UserDto> AuthenticateAsync(string email, string password);
        public Task DeleteUserAsync(int id);
        public Task<List<BookTypeDto>> GetAllBookTypesAsync();
        public Task UpdateUserAsync(UserDto dto);

    }
}
