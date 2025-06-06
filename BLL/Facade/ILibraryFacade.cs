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
        public Task<IEnumerable<BookDto>> ReadAllBooksAsync();
        public Task<BookDto> ReadBookByIdAsync(int id);
        public Task CreateBookAsync(ActionBookDto dto);
        public Task UpdateBookAsync(int bookId, ActionBookDto dto);
        public Task<List<BookDto>> ReadBooksByTypeAsync(int typeId);
        public Task DeleteBookAsync(int id);
        public Task<IEnumerable<GenreDto>> ReadAllGenresAsync();
        public Task<GenreDto> ReadGenreByIdAsync(int id);
        public Task CreateGenreAsync(GenreDto dto);
        public Task UpdateGenreAsync(GenreDto dto);
        public Task DeleteGenreAsync(int id);
        public Task<IEnumerable<OrderDto>> ReadAllOrdersAsync();
        public Task<OrderDto> ReadOrderByIdAsync(int id);
        public Task<IEnumerable<OrderDto>> ReadOrdersByUserAsync(int userId);
        public Task CreateOrderAsync(ActionOrderDto dto);
        public Task UpdateStatusAsync(int orderId);
        public Task DeleteOrderAsync(int id, bool isAdmin);
        public Task<UserDto> RegisterUserAsync(UserDto dto);
        public Task<UserDto> ReadUserByIdAsync(int id);
        Task<IEnumerable<UserDto>> ReadAllUsersAsync();
        public Task<UserDto> AuthenticateAsync(string email, string password);
        public Task DeleteUserAsync(int id);
        public Task<List<BookTypeDto>> ReadAllBookTypesAsync();
        public Task UpdateUserAsync(UserDto dto);

    }
}
