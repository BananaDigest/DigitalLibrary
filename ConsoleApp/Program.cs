using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BLL.Facade;
using BLL.Interfaces;
using BLL.Services;
using BLL.Mapping;
using DAL.Context;
using DAL.UnitOfWork;
using Domain.Enums;
using BLL.DTOs;
using Microsoft.Extensions.Configuration;
using Domain.Entities;
using BLL.Factory;

namespace ConsoleLibraryApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Зчитуємо конфігурацію з appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();

            // DbContext та UnitOfWork з конфігурації
            services.AddDbContext<DigitalLibraryContext>(opt =>
                opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IBookFactory, BookFactory>();

            // AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

            // BLL сервіси
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IGenreService, GenreService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IBookTypeService, BookTypeService>();

            // Фасад
            services.AddScoped<LibraryFacade>();

            var provider = services.BuildServiceProvider();
            var facade = provider.GetRequiredService<LibraryFacade>();

            try
            {
                await RunConsoleAsync(facade);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }

        static async Task RunConsoleAsync(LibraryFacade f)
        {
            UserDto current = null;
            while (true)
            {
                Console.Clear();
                if (current == null)
                {
                    Console.WriteLine("=== Guest Menu ===");
                    Console.WriteLine("1) View Catalog");
                    Console.WriteLine("2) Search Books");
                    Console.WriteLine("3) Filter by Type");
                    Console.WriteLine("4) Filter by Genre");
                    Console.WriteLine("5) Register");
                    Console.WriteLine("6) Login");
                    Console.WriteLine("0) Exit");
                    Console.Write("Select: ");
                    var opt = Console.ReadLine();
                    switch (opt)
                    {
                        case "1": await ViewCatalog(f); break;
                        case "2": await SearchBooks(f); break;
                        case "3": await FilterByType(f); break;
                        case "4": await FilterByGenre(f); break;
                        case "5": current = await Register(f); break;
                        case "6": current = await Login(f); break;
                        case "0": return;
                        default: Console.WriteLine("Invalid"); break;
                    }
                }
                else
                {
                    Console.WriteLine($"=== {current.Role} Menu ({current.Email}) ===");
                    Console.WriteLine("1) View Catalog");
                    Console.WriteLine("2) Search Books");
                    Console.WriteLine("3) Place Order");
                    if (current.Role == nameof(UserRole.Manager) || current.Role == nameof(UserRole.Administrator))
                        Console.WriteLine("4) Manage Books");
                    if (current.Role == nameof(UserRole.Manager))
                        Console.WriteLine("6) Manage Genres");
                    if (current.Role == nameof(UserRole.Administrator))
                        Console.WriteLine("5) Manage Orders");
                    Console.WriteLine("9) Logout");
                    Console.WriteLine("0) Exit");
                    Console.Write("Select: ");
                    var opt = Console.ReadLine();
                    switch (opt)
                    {
                        case "1": await ViewCatalog(f); break;
                        case "2": await SearchBooks(f); break;
                        case "3": await PlaceOrder(f, current.Id); break;
                        case "4": await ManageBooks(f); break;
                        case "5": await ManageOrders(f); break;
                        case "6": await ManageGenres(f); break;
                        case "9": current = null; break;
                        case "0": return;
                        default: Console.WriteLine("Invalid"); break;
                    }
                }
                Console.WriteLine("Press any key...");
                Console.ReadKey();
            }
        }

        static async Task ViewCatalog(LibraryFacade f)
        {
            var books = await f.GetAllBooksAsync();
            foreach (var b in books)
                Console.WriteLine($"{b.Id} | {b.Title} | {b.Author} | {b.Publisher} | Types: {b.AvailableTypeIds}");
        }

        static async Task SearchBooks(LibraryFacade f)
        {
            Console.Write("Enter search term: ");
            var term = Console.ReadLine();
            var books = await f.SearchBooksAsync(term);
            foreach (var b in books)
                Console.WriteLine($"{b.Id} | {b.Title} | {b.Author}");
        }

        static async Task FilterByType(LibraryFacade f)
        {
            Console.WriteLine("Select type: 1) Paper 2) Audio 3) Electronic");
            int typeId = int.Parse(Console.ReadLine());
            var books = await f.FilterBooksByTypeAsync(typeId);
            foreach (var b in books)
                Console.WriteLine($"{b.Id} | {b.Title} | {b.AvailableTypeIds}");
        }

        static async Task FilterByGenre(LibraryFacade f)
        {
            var genres = await f.GetAllGenresAsync();
            Console.WriteLine("Available genres:");
            foreach (var g in genres)
                Console.WriteLine($"{g.Id} - {g.Name}");
            Console.Write("Enter genre ID: ");
            var id = int.Parse(Console.ReadLine());
            var books = await f.FilterBooksByGenreAsync(id);
            foreach (var b in books)
                Console.WriteLine($"{b.Id} | {b.Title} | GenreId: {b.GenreId}");
        }

        static async Task<UserDto> Register(LibraryFacade f)
        {
            var dto = new UserDto();
            Console.Write("FirstName: "); dto.FirstName = Console.ReadLine();
            Console.Write("LastName: "); dto.LastName = Console.ReadLine();
            Console.Write("Email: "); dto.Email = Console.ReadLine();
            Console.Write("Password: "); dto.Password = Console.ReadLine();
            return await f.RegisterUserAsync(dto);
        }

        static async Task<UserDto> Login(LibraryFacade f)
        {
            Console.Write("Email: "); var email = Console.ReadLine();
            Console.Write("Password: "); var pwd = Console.ReadLine();
            return await f.AuthenticateAsync(email, pwd);
        }

        static async Task PlaceOrder(LibraryFacade f, int userId)
        {
            Console.Write("BookId: "); var bid = int.Parse(Console.ReadLine());
            Console.WriteLine("Select type: 1) Paper 2) Audio 3) Electronic");
            var opt = Console.ReadLine();
            var type = opt switch
            {
                "1" => BookType.Paper,
                "2" => BookType.Audio,
                _ => BookType.Electronic
            };
            var dto = new ActionOrderDto { UserId = userId, BookId = bid, OrderType = type };
            await f.CreateOrderAsync(dto);
            Console.WriteLine("Order placed");
        }

        static async Task ManageBooks(LibraryFacade f)
        {
            Console.WriteLine("1) Create 2) Update 3) Delete");
            var opt = Console.ReadLine();
            switch (opt)
            {
                case "1": await CreateBook(f); break;
                case "2": await UpdateBook(f); break;
                case "3": await DeleteBook(f); break;
                case "0": return;
                default: Console.WriteLine("Invalid"); break;
            }

        }

        static async Task CreateBook(LibraryFacade f)
        {
            var dto = new ActionBookDto();
            Console.Write("Enter Title: ");
            dto.Title = Console.ReadLine() ?? string.Empty;
            Console.Write("Enter Author: ");
            dto.Author = Console.ReadLine() ?? string.Empty;
            Console.Write("Enter Publisher: ");
            dto.Publisher = Console.ReadLine() ?? string.Empty;
            Console.Write("Enter PublicationYear: ");
            dto.PublicationYear = int.TryParse(Console.ReadLine(), out var year) ? year : 0;

            // --- Ось тут: виводимо всі доступні типи BookTypeEntity
            var types = await f.GetAllBookTypesAsync(); // має повернути List<BookTypeDto> або List<Type>
            foreach (var t in types)
                Console.WriteLine($"{t.Id} | {t.Name}");
            Console.Write("Enter AvailableTypeIds (comma-separated): ");
            var rawTypes = Console.ReadLine() ?? string.Empty;
            dto.AvailableTypeIds = rawTypes
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : -1)
                .Where(id => id > 0)
                .ToList();

            // Якщо серед AvailableTypeIds є (int)BookType.Paper, то питаємо CopyCount
            if (dto.AvailableTypeIds.Contains((int)BookType.Paper))
            {
                Console.Write("Enter number of paper copies: ");
                dto.CopyCount = int.TryParse(Console.ReadLine(), out var cnt) ? cnt : 0;
            }
            // Інакше CopyCount лишається 0 (за замовчуванням в DTO)

            // --- Ось тут: виводимо жанри
            var genres = await f.GetAllGenresAsync();
            foreach (var g in genres)
                Console.WriteLine($"{g.Id} | {g.Name}");
            Console.Write("Enter GenreId: ");
            dto.GenreId = int.TryParse(Console.ReadLine(), out var gid) ? gid : 0;

            await f.CreateBookAsync(dto);
            Console.WriteLine("Book created.");
        }

        static async Task UpdateBook(LibraryFacade f)
        {
            // 1) Виводимо список усіх книг з консольними полями і AvailableTypeIds
            var books = await f.GetAllBooksAsync();
            Console.WriteLine("=== Список усіх книг ===");
            foreach (var b in books)
            {
                Console.WriteLine($"{b.Id} | {b.Title} | {b.Author} | {b.Publisher} | Types: {string.Join(", ", b.AvailableTypeIds)}");
            }

            try
            {
                // 2) Запитуємо у користувача Id тієї книги, яку хочемо оновити
                Console.Write("Enter Book Id to update: ");
                if (!int.TryParse(Console.ReadLine(), out var bookId))
                {
                    Console.WriteLine("Невірний формат Id. Повертаємось у меню.\n");
                    return;
                }

                // 3) Завантажуємо деталі обраної книги через фасад
                var existing = await f.GetBookByIdAsync(bookId);
                if (existing == null)
                {
                    Console.WriteLine($"Книга з Id = {bookId} не знайдена.\n");
                    return;
                }

                // 4) Виводимо поточні значення полів обраної книги
                Console.WriteLine($"Current Title: {existing.Title}");
                Console.WriteLine($"Current Author: {existing.Author}");
                Console.WriteLine($"Current Publisher: {existing.Publisher}");
                Console.WriteLine($"Current PublicationYear: {existing.PublicationYear}");
                Console.WriteLine($"Current GenreId: {existing.GenreId}");
                Console.WriteLine($"Current AvailableTypeIds: {string.Join(", ", existing.AvailableTypeIds)}");
                Console.WriteLine($"Current CopyCount (якщо є копії): {existing.AvailableCopies}");
                Console.WriteLine("-------------------------------------------------------");

                // 5) Через DAL (фасад) отримуємо всю таблицю з типами книг
                var allTypes = await f.GetAllBookTypesAsync(); // повертає List<BookTypeDto>
                                                               // 6) Фільтруємо саме ті BookTypeEntity, Id яких міститься в existing.AvailableTypeIds
                var matchedTypes = allTypes
                    .Where(t => existing.AvailableTypeIds.Contains(t.Id))
                    .ToList();

                // 7) Виводимо знайдені типи для цієї книги
                Console.WriteLine("Existing Types (з таблиці BookTypes):");
                foreach (var t in matchedTypes)
                {
                    Console.WriteLine($"  {t.Id} | {t.Name}");
                }
                Console.WriteLine("-------------------------------------------------------");

                // 8) Тепер просимо ввести нові дані для оновлення
                Console.Write("Enter new Title: ");
                var title = Console.ReadLine()?.Trim();

                Console.Write("Enter new Author: ");
                var author = Console.ReadLine()?.Trim();

                Console.Write("Enter new Publisher: ");
                var publisher = Console.ReadLine()?.Trim();

                Console.Write("Enter new PublicationYear: ");
                if (!int.TryParse(Console.ReadLine(), out var year))
                {
                    Console.WriteLine("Невірний формат року. Повертаємось у меню.\n");
                    return;
                }

                Console.Write("Enter new GenreId: ");
                if (!int.TryParse(Console.ReadLine(), out var genreId))
                {
                    Console.WriteLine("Невірний формат GenreId. Повертаємось у меню.\n");
                    return;
                }

                Console.Write("Enter AvailableTypeIds (comma-separated, напр.: 0,1): ");
                var rawTypes = Console.ReadLine() ?? "";
                var typeIds = rawTypes
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .Distinct()
                    .ToList();

                if (typeIds.Count == 0)
                {
                    Console.WriteLine("Принаймні один тип має бути задано. Повертаємось у меню.\n");
                    return;
                }

                int copyCount = 0;
                if (typeIds.Contains((int)BookType.Paper))
                {
                    Console.Write("Enter new CopyCount (кількість паперових копій): ");
                    if (!int.TryParse(Console.ReadLine(), out copyCount) || copyCount < 1)
                    {
                        Console.WriteLine("Невірний формат CopyCount. Повертаємось у меню.\n");
                        return;
                    }
                }

                // 9) Формуємо DTO для оновлення
                var dto = new ActionBookDto
                {
                    Title = title,
                    Author = author,
                    Publisher = publisher,
                    PublicationYear = year,
                    GenreId = genreId,
                    AvailableTypeIds = typeIds,
                    CopyCount = copyCount
                };

                // 10) Викликаємо метод оновлення через фасад
                await f.UpdateBookAsync(bookId, dto);

                Console.WriteLine($"Книга з Id = {bookId} успішно оновлена.\n");
            }
            catch (KeyNotFoundException knf)
            {
                Console.WriteLine($"Помилка: {knf.Message}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Сталася помилка під час оновлення: {ex.Message}\n");
            }
        }
        static async Task DeleteBook(LibraryFacade facade)
        {
            try
            {
                Console.Write("Enter Book Id to delete: ");
                if (!int.TryParse(Console.ReadLine(), out var bookId))
                {
                    Console.WriteLine("Невірний формат Id. Повертаємось у меню.\n");
                    return;
                }

                // Опційна перевірка:
                var existing = await facade.GetBookByIdAsync(bookId);
                if (existing == null)
                {
                    Console.WriteLine($"Книга з Id = {bookId} не знайдена.\n");
                    return;
                }

                await facade.DeleteBookAsync(bookId);
                Console.WriteLine($"Книга з Id = {bookId} успішно видалена.\n");
            }
            catch (KeyNotFoundException knf)
            {
                Console.WriteLine($"Помилка: {knf.Message}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Сталася помилка під час видалення: {ex.Message}\n");
            }
        }

        static async Task ManageOrders(LibraryFacade f)
        {
            Console.WriteLine("1) View All 2) Delete");
            var opt = Console.ReadLine();
            //TODO Реалізувати перегляд через f.GetAllOrdersAsync та видалення через f.DeleteOrderAsync
        }

        private static async Task ManageGenres(LibraryFacade f)
        {
            Console.WriteLine("1) Create Genre 2) Update Genre 3) Delete Genre");
            var opt = Console.ReadLine();
            switch (opt)
            {
                case "1": await CreateGenre(f); break;
                case "2": await UpdateGenre(f); break;
                case "3": await DeleteGenre(f); break;
                case "0": return;
                default: Console.WriteLine("Invalid"); break;
            }
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static async Task CreateGenre(LibraryFacade f)
        {
            Console.Write("Назва жанру: "); var name = Console.ReadLine();
            await f.CreateGenreAsync(new GenreDto { Name = name });
            Console.WriteLine("Жанр додано.");
        }

        private static async Task UpdateGenre(LibraryFacade f)
        {
            var genres = await f.GetAllGenresAsync();
            foreach (var g in genres)
                Console.WriteLine($"{g.Id} | {g.Name} ");
            Console.Write("Genre ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid ID"); return; }
            var genre = await f.GetGenreByIdAsync(id);
            Console.WriteLine($"Поточна назва: {genre.Name}");
            Console.Write("Нова назва: "); var nm = Console.ReadLine();
            genre.Name = nm;
            await f.UpdateGenreAsync(genre);
            Console.WriteLine("Жанр оновлено.");
        }

        private static async Task DeleteGenre(LibraryFacade f)
        {
            Console.Write("Genre ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid ID"); return; }
            await f.DeleteGenreAsync(id);
            Console.WriteLine("Жанр видалено.");
        }
    }
}
