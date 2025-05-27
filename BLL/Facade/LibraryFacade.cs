using BLL.DTOs;
using BLL.Factory;
using BLL.Interfaces;
using BLL.Repositories;
using BLL.Strategy;
using Domain.Entities;
using Domain.Enums;
using BLL.Services;

namespace BLL.Facade
{
    public class LibraryFacade
    {
        private IBookService _bookService;
        private IOrderService _orderService;
        private IBookRepository _bookRepository;

        public LibraryFacade(
            IBookService bookService,
            IOrderService orderService,
            IBookRepository bookRepository)
        {
            _bookService = bookService;
            _orderService = orderService;
            _bookRepository = bookRepository;
        }

        // 1. Додати книгу через фабрику
        public async Task<Guid> CreateBookAsync(ActionBookDto dto)
        {
            BookFactory factory = dto.AvailableTypes.HasFlag(BookType.Paper)
                ? (BookFactory)new PaperBookFactory()
                : dto.AvailableTypes.HasFlag(BookType.Audio)
                    ? new AudioBookFactory()
                    : new ElectronicBookFactory();

            var book = factory.Create(dto);
            await _bookService.CreateAsync(dto);
            return book.Id;
        }

        // 2. Пошук книги за критерієм
        public async Task<IEnumerable<ActionBookDto>> FindBooksAsync(string value, string filterBy)
        {
            IBookFilterStrategy strategy = filterBy.ToLower() switch
            {
                "author" => new AuthorFilterStrategy(_bookRepository),
                _ => new TitleFilterStrategy(_bookRepository)
            };

            var books = await strategy.FilterAsync(value);
            return books.Select(b => new ActionBookDto
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
            // тут ти можеш додати будь-яку додаткову логіку (авторизація, логування тощо)
            await _bookService.DeleteAsync(bookId);
        }

        // 3. Створити замовлення
        public async Task CreateOrderAsync(ActionOrderDto dto)
        {
            await _orderService.CreateAsync(dto);
        }

        // 4. Видалити замовлення
        public async Task DeleteOrderAsync(Guid id)
        {
            await _orderService.DeleteAsync(id);
        }

        // 5. Отримати доступні копії книги
        public async Task<IEnumerable<BookCopy>> ReadAvailableCopiesAsync(Guid bookId)
        {
            var book = await _bookRepository.ReadByIdAsync(bookId);
            return book?.Copies?.Where(c => c.IsAvailable) ?? Enumerable.Empty<BookCopy>();
        }

        // 6. Звіт по типах книг
        public async Task<Dictionary<BookType, int>> ReadReportByTypesAsync()
        {
            var books = await _bookRepository.ReadAllAsync();
            return books
                .GroupBy(b => b.AvailableTypes)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
