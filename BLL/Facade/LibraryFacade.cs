using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Interfaces;
using BLL.Services;
using Domain.Enums;

namespace BLL.Facade
{
    /// <summary>
    /// Фасад, який інкапсулює взаємодію з різними сервісами BLL
    /// та спрощує клієнтський код (ConsoleApp, Web API тощо).
    /// </summary>
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

        // Книги
        /// <summary>Повернути всі книги.</summary>
        public Task<IEnumerable<BookDto>> ReadAllBooksAsync() =>
            _bookService.ReadAllAsync();

        /// <summary>Повернути книгу за ідентифікатором.</summary>
        public Task<BookDto> ReadBookByIdAsync(int id) =>
            _bookService.ReadByIdAsync(id);

        /// <summary>Створити нову книгу.</summary>
        public Task CreateBookAsync(ActionBookDto dto) =>
            _bookService.CreateAsync(dto);

        /// <summary>Оновити існуючу книгу.</summary>
        public Task UpdateBookAsync(int bookId, ActionBookDto dto) =>
            _bookService.UpdateAsync(bookId, dto);

        public async Task<List<BookDto>> ReadBooksByTypeAsync(int typeId)=>
            await _bookService.ReadByTypeAsync(typeId);

        /// <summary>Видалити книгу.</summary>
        public Task DeleteBookAsync(int id) =>
            _bookService.DeleteAsync(id);

        // Жанри
        /// <summary>Повернути всі жанри.</summary>
        public Task<IEnumerable<GenreDto>> ReadAllGenresAsync() =>
            _genreService.ReadAllAsync();

        /// <summary>Повернути жанр за ідентифікатором.</summary>
        public Task<GenreDto> ReadGenreByIdAsync(int id) =>
            _genreService.ReadByIdAsync(id);

        /// <summary>Створити новий жанр.</summary>
        public Task CreateGenreAsync(GenreDto dto) =>
            _genreService.CreateAsync(dto);

        /// <summary>Оновити жанр.</summary>
        public Task UpdateGenreAsync(GenreDto dto) =>
            _genreService.UpdateAsync(dto);

        /// <summary>Видалити жанр.</summary>
        public Task DeleteGenreAsync(int id) =>
            _genreService.DeleteAsync(id);

        // Замовлення
        /// <summary>Повернути всі замовлення.</summary>
        public Task<IEnumerable<OrderDto>> ReadAllOrdersAsync() =>
            _orderService.ReadAllAsync();

        /// <summary>Повернути замовлення за ідентифікатором.</summary>
        public Task<OrderDto> ReadOrderByIdAsync(int id) =>
            _orderService.ReadByIdAsync(id);

        /// <summary>Повернути замовлення користувача.</summary>
        public Task<IEnumerable<OrderDto>> ReadOrdersByUserAsync(int userId) =>
            _orderService.ReadByUserAsync(userId);

        /// <summary>Створити нове замовлення.</summary>
        public Task CreateOrderAsync(ActionOrderDto dto) =>
            _orderService.CreateAsync(dto);

        public Task UpdateStatusAsync(int orderId) =>
            _orderService.UpdateStatusAsync(orderId);

        /// <summary>Видалити замовлення.</summary>
        public Task DeleteOrderAsync(int id, bool isAdmin) =>
            _orderService.DeleteAsync(id, isAdmin);

        // Користувачі
        /// <summary>Зареєструвати користувача.</summary>
        public async Task<UserDto> RegisterUserAsync(UserDto dto)
        {
            await _userService.RegisterAsync(dto);
            return dto;
        }

        /// <summary>Отримати користувача за ідентифікатором.</summary>
        public Task<UserDto> ReadUserByIdAsync(int id) =>
            _userService.ReadByIdAsync(id);

        /// <summary>Отримати всіх користувачів (без паролів).</summary>
        public Task<IEnumerable<UserDto>> ReadAllUsersAsync() =>
            _userService.ReadAllUsersAsync();

        /// <summary>Аутентифікувати користувача.</summary>
        public Task<UserDto> AuthenticateAsync(string email, string password) =>
            _userService.AuthenticateAsync(email, password);

        /// <summary>Видалити користувача.</summary>
        public Task DeleteUserAsync(int id) =>
            _userService.DeleteAsync(id);

        public Task<List<BookTypeDto>> ReadAllBookTypesAsync()
        => _bookTypeService.ReadAllBookTypesAsync();

        public async Task UpdateUserAsync(UserDto dto)
            => await _userService.UpdateUserAsync(dto);
    }
}
