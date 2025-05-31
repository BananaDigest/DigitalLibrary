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
    public class LibraryFacade
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
        public Task<IEnumerable<BookDto>> GetAllBooksAsync() =>
            _bookService.ReadAllAsync();

        /// <summary>Повернути книгу за ідентифікатором.</summary>
        public Task<BookDto> GetBookByIdAsync(int id) =>
            _bookService.ReadByIdAsync(id);

        /// <summary>Пошук книг за терміном.</summary>
        public Task<IEnumerable<BookDto>> SearchBooksAsync(string term) =>
            _bookService.SearchAsync(term);

        /// <summary>Фільтрація книг за типом.</summary>
        public Task<IEnumerable<BookDto>> FilterBooksByTypeAsync(int typeId)
    => _bookService.FilterByTypeAsync(typeId);

        /// <summary>Фільтрація книг за жанром.</summary>
        public Task<IEnumerable<BookDto>> FilterBooksByGenreAsync(int genreId) =>
            _bookService.FilterByGenreAsync(genreId);

        /// <summary>Створити нову книгу.</summary>
        public Task CreateBookAsync(ActionBookDto dto) =>
            _bookService.CreateAsync(dto);

        /// <summary>Оновити існуючу книгу.</summary>
        public Task UpdateBookAsync(ActionBookDto dto) =>
            _bookService.UpdateAsync(dto);

        /// <summary>Видалити книгу.</summary>
        public Task DeleteBookAsync(int id) =>
            _bookService.DeleteAsync(id);

        // Жанри
        /// <summary>Повернути всі жанри.</summary>
        public Task<IEnumerable<GenreDto>> GetAllGenresAsync() =>
            _genreService.ReadAllAsync();

        /// <summary>Повернути жанр за ідентифікатором.</summary>
        public Task<GenreDto> GetGenreByIdAsync(int id) =>
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
        public Task<IEnumerable<OrderDto>> GetAllOrdersAsync() =>
            _orderService.ReadAllAsync();

        /// <summary>Повернути замовлення за ідентифікатором.</summary>
        public Task<OrderDto> GetOrderByIdAsync(int id) =>
            _orderService.ReadByIdAsync(id);

        /// <summary>Повернути замовлення користувача.</summary>
        public Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId) =>
            _orderService.ReadByUserAsync(userId);

        /// <summary>Створити нове замовлення.</summary>
        public Task CreateOrderAsync(ActionOrderDto dto) =>
            _orderService.CreateAsync(dto);

        /// <summary>Оновити замовлення.</summary>
        public Task UpdateOrderAsync(ActionOrderDto dto) =>
            _orderService.UpdateAsync(dto);

        /// <summary>Видалити замовлення.</summary>
        public Task DeleteOrderAsync(int id) =>
            _orderService.DeleteAsync(id);

        // Користувачі
        /// <summary>Зареєструвати користувача.</summary>
        public async Task<UserDto> RegisterUserAsync(UserDto dto)
        {
            await _userService.RegisterAsync(dto);
            return dto;
        }

        /// <summary>Отримати користувача за ідентифікатором.</summary>
        public Task<UserDto> GetUserByIdAsync(int id) =>
            _userService.ReadByIdAsync(id);

        /// <summary>Аутентифікувати користувача.</summary>
        public Task<UserDto> AuthenticateAsync(string email, string password) =>
            _userService.AuthenticateAsync(email, password);

        /// <summary>Видалити користувача.</summary>
        public Task DeleteUserAsync(int id) =>
            _userService.DeleteAsync(id);

        public Task<List<BookTypeDto>> GetAllBookTypesAsync()
        => _bookTypeService.GetAllBookTypesAsync();
    }
}
