using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Interfaces;
using BLL.Services;
using Domain.Enums;

namespace BLL.Facade
{
    public class LibraryFacade: ILibraryFacade
    {
        private readonly IBookService _bookService;
        private readonly IGenreService _genreService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IBookTypeService _bookTypeService;

        public LibraryFacade(
            IBookService bookService,
            IGenreService genreService,
            IOrderService orderService,
            IUserService userService, 
            IBookTypeService bookTypeService)
        {
            _bookService = bookService;
            _genreService = genreService;
            _orderService = orderService;
            _userService = userService;
            _bookTypeService = bookTypeService;
        }

        public Task<IEnumerable<BookDto>> ReadAllBooksAsync() =>
            _bookService.ReadAllAsync();

        public Task<BookDto> ReadBookByIdAsync(int id) =>
            _bookService.ReadByIdAsync(id);

        public Task CreateBookAsync(ActionBookDto dto) =>
            _bookService.CreateAsync(dto);

        public Task UpdateBookAsync(int bookId, ActionBookDto dto) =>
            _bookService.UpdateAsync(bookId, dto);

        public async Task<List<BookDto>> ReadBooksByTypeAsync(int typeId)=>
            await _bookService.ReadByTypeAsync(typeId);

        public Task DeleteBookAsync(int id) =>
            _bookService.DeleteAsync(id);

        public Task<IEnumerable<GenreDto>> ReadAllGenresAsync() =>
            _genreService.ReadAllAsync();

        public Task<GenreDto> ReadGenreByIdAsync(int id) =>
            _genreService.ReadByIdAsync(id);

        public Task CreateGenreAsync(GenreDto dto) =>
            _genreService.CreateAsync(dto);

        public Task UpdateGenreAsync(GenreDto dto) =>
            _genreService.UpdateAsync(dto);

        public Task DeleteGenreAsync(int id) =>
            _genreService.DeleteAsync(id);

        public Task<IEnumerable<OrderDto>> ReadAllOrdersAsync() =>
            _orderService.ReadAllAsync();

        public Task<OrderDto> ReadOrderByIdAsync(int id) =>
            _orderService.ReadByIdAsync(id);

        public Task<IEnumerable<OrderDto>> ReadOrdersByUserAsync(int userId) =>
            _orderService.ReadByUserAsync(userId);

        public Task CreateOrderAsync(ActionOrderDto dto) =>
            _orderService.CreateAsync(dto);

        public Task UpdateStatusAsync(int orderId) =>
            _orderService.UpdateStatusAsync(orderId);

        public Task DeleteOrderAsync(int id, bool isAdmin) =>
            _orderService.DeleteAsync(id, isAdmin);

        public async Task<UserDto> RegisterUserAsync(UserDto dto)
        {
            await _userService.RegisterAsync(dto);
            return dto;
        }

        public Task<UserDto> ReadUserByIdAsync(int id) =>
            _userService.ReadByIdAsync(id);

        public Task<IEnumerable<UserDto>> ReadAllUsersAsync() =>
            _userService.ReadAllUsersAsync();

        public Task<UserDto> AuthenticateAsync(string email, string password) =>
            _userService.AuthenticateAsync(email, password);

        public Task DeleteUserAsync(int id) =>
            _userService.DeleteAsync(id);

        public Task<List<BookTypeDto>> ReadAllBookTypesAsync()
        => _bookTypeService.ReadAllBookTypesAsync();

        public async Task UpdateUserAsync(UserDto dto)
            => await _userService.UpdateUserAsync(dto);
    }
}
