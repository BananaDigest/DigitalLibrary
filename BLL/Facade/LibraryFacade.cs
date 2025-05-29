using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Factory;
using BLL.Interfaces;
using Domain.Enums;

namespace BLL.Facade
{
    public class LibraryFacade
    {
        private readonly IBookService _bookService;
        private readonly IOrderService _orderService;
        private readonly IGenreService _genreService;
        private readonly IUserService _userService;

        public LibraryFacade(
            IBookService bookService,
            IOrderService orderService,
            IGenreService genreService,
            IUserService userService)
        {
            _bookService = bookService;
            _orderService = orderService;
            _genreService = genreService;
            _userService = userService;
        }

        // 1. Додати книгу через фабрику та сервіс
        public async Task<Guid> CreateBookAsync(ActionBookDto dto)
        {
            // Фабрика створює доменну модель для отримання Id
            BookFactory factory = dto.AvailableTypes.HasFlag(BookType.Paper)
                ? (BookFactory)new PaperBookFactory()
                : dto.AvailableTypes.HasFlag(BookType.Audio)
                    ? new AudioBookFactory()
                    : new ElectronicBookFactory();

            var domainBook = factory.Create(dto);

            // Виклик сервісу для збереження через BLL
            await _bookService.CreateAsync(dto);

            // Повертаємо Id з доменної моделі
            return domainBook.Id;
        }

        // 2. Пошук книг за автором або назвою через сервіс
        public async Task<IEnumerable<ActionBookDto>> FindBooksAsync(string value, string filterBy)
        {
            var all = await _bookService.ReadAllAsync();
            IEnumerable<BookDto> filtered = filterBy.ToLower() switch
            {
                "author" => all.Where(b => b.Author.Contains(value, StringComparison.OrdinalIgnoreCase)),
                _ => all.Where(b => b.Title.Contains(value, StringComparison.OrdinalIgnoreCase)),
            };

            return filtered.Select(b => new ActionBookDto
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Publisher = b.Publisher,
                PublicationYear = b.PublicationYear,
                AvailableTypes = b.AvailableTypes,
                GenreId = b.GenreId
            });
        }

        public async Task DeleteBookAsync(Guid bookId)
        {
            await _bookService.DeleteAsync(bookId);
        }

        // 3. Створення замовлення через сервіс
        public async Task CreateOrderAsync(ActionOrderDto dto)
        {
            await _orderService.CreateAsync(dto);
        }

        // 4. Видалення замовлення через сервіс
        public async Task DeleteOrderAsync(Guid id)
        {
            await _orderService.DeleteAsync(id);
        }

        // 5. Отримати доступні копії книги через сервіс
        public async Task<int> ReadAvailableCopiesAsync(Guid bookId)
        {
            // Оскільки в BookDto є лише AvailableCopies (int)
            var bookDto = await _bookService.ReadByIdAsync(bookId);
            return bookDto.AvailableCopies;
        }

        // 6. Звіт по типах книг через сервіс
        public async Task<Dictionary<BookType, int>> ReadReportByTypesAsync()
        {
            var all = await _bookService.ReadAllAsync();
            return all
                .GroupBy(b => b.AvailableTypes)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        // 7. Приклад роботи з жанрами через IGenreService
        public async Task<IEnumerable<GenreDto>> GetAllGenresAsync()
        {
            return await _genreService.ReadAllAsync();
        }

        // 8. Приклад роботи з користувачами через IUserService
        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            return await _userService.ReadByIdAsync(userId);
        }
    }
}
