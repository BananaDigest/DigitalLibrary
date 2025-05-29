using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Interfaces;
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

        public LibraryFacade(
            IBookService bookService,
            IGenreService genreService,
            IOrderService orderService,
            IUserService userService)
        {
            _bookService = bookService;
            _genreService = genreService;
            _orderService = orderService;
            _userService = userService;
        }

        // Книги
        /// <summary>Повернути всі книги.</summary>
        public Task<IEnumerable<BookDto>> GetAllBooksAsync() =>
            _bookService.ReadAllAsync();

        /// <summary>Повернути книгу за ідентифікатором.</summary>
        public Task<BookDto> GetBookByIdAsync(Guid id) =>
            _bookService.ReadByIdAsync(id);

        /// <summary>Пошук книг за терміном.</summary>
        public Task<IEnumerable<BookDto>> SearchBooksAsync(string term) =>
            _bookService.SearchAsync(term);

        /// <summary>Фільтрація книг за типом.</summary>
        public Task<IEnumerable<BookDto>> FilterBooksByTypeAsync(BookType type) =>
            _bookService.FilterByTypeAsync(type);

        /// <summary>Фільтрація книг за жанром.</summary>
        public Task<IEnumerable<BookDto>> FilterBooksByGenreAsync(Guid genreId) =>
            _bookService.FilterByGenreAsync(genreId);

        /// <summary>Створити нову книгу.</summary>
        public Task CreateBookAsync(ActionBookDto dto) =>
            _bookService.CreateAsync(dto);

        /// <summary>Оновити існуючу книгу.</summary>
        public Task UpdateBookAsync(ActionBookDto dto) =>
            _bookService.UpdateAsync(dto);

        /// <summary>Видалити книгу.</summary>
        public Task DeleteBookAsync(Guid id) =>
            _bookService.DeleteAsync(id);

        // Жанри
        /// <summary>Повернути всі жанри.</summary>
        public Task<IEnumerable<GenreDto>> GetAllGenresAsync() =>
            _genreService.ReadAllAsync();

        /// <summary>Повернути жанр за ідентифікатором.</summary>
        public Task<GenreDto> GetGenreByIdAsync(Guid id) =>
            _genreService.ReadByIdAsync(id);

        /// <summary>Створити новий жанр.</summary>
        public Task CreateGenreAsync(GenreDto dto) =>
            _genreService.CreateAsync(dto);

        /// <summary>Оновити жанр.</summary>
        public Task UpdateGenreAsync(GenreDto dto) =>
            _genreService.UpdateAsync(dto);

        /// <summary>Видалити жанр.</summary>
        public Task DeleteGenreAsync(Guid id) =>
            _genreService.DeleteAsync(id);

        // Замовлення
        /// <summary>Повернути всі замовлення.</summary>
        public Task<IEnumerable<OrderDto>> GetAllOrdersAsync() =>
            _orderService.ReadAllAsync();

        /// <summary>Повернути замовлення за ідентифікатором.</summary>
        public Task<OrderDto> GetOrderByIdAsync(Guid id) =>
            _orderService.ReadByIdAsync(id);

        /// <summary>Повернути замовлення користувача.</summary>
        public Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(Guid userId) =>
            _orderService.ReadByUserAsync(userId);

        /// <summary>Створити нове замовлення.</summary>
        public Task CreateOrderAsync(ActionOrderDto dto) =>
            _orderService.CreateAsync(dto);

        /// <summary>Оновити замовлення.</summary>
        public Task UpdateOrderAsync(ActionOrderDto dto) =>
            _orderService.UpdateAsync(dto);

        /// <summary>Видалити замовлення.</summary>
        public Task DeleteOrderAsync(Guid id) =>
            _orderService.DeleteAsync(id);

        // Користувачі
        /// <summary>Зареєструвати користувача.</summary>
        public async Task<UserDto> RegisterUserAsync(UserDto dto)
        {
            await _userService.RegisterAsync(dto);
            return dto;
        }

        /// <summary>Отримати користувача за ідентифікатором.</summary>
        public Task<UserDto> GetUserByIdAsync(Guid id) =>
            _userService.ReadByIdAsync(id);

        /// <summary>Аутентифікувати користувача.</summary>
        public Task<UserDto> AuthenticateAsync(string email, string password) =>
            _userService.AuthenticateAsync(email, password);

        /// <summary>Видалити користувача.</summary>
        public Task DeleteUserAsync(Guid id) =>
            _userService.DeleteAsync(id);
    }
}
