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
            services.AddScoped<ILibraryFacade, LibraryFacade>();

            var provider = services.BuildServiceProvider();
            var facade = provider.GetRequiredService<ILibraryFacade>();

            try
            {
                await RunConsoleAsync(facade);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }

        static async Task RunConsoleAsync(ILibraryFacade f)
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
                    if (current.Role == nameof(UserRole.Registered))
                        Console.WriteLine("7) Update Profile");
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
                        case "7": await UpdateProfile(f); break;
                        case "9": current = null; break;
                        case "0": return;
                        default: Console.WriteLine("Invalid"); break;
                    }
                }
                Console.WriteLine("Press any key...");
                Console.ReadKey();
            }
        }

        static async Task ViewCatalog(ILibraryFacade f)
        {
            var books = await f.GetAllBooksAsync();
            foreach (var b in books)
                Console.WriteLine($"{b.Id} | {b.Title} | {b.Author} | {b.Publisher} | Types: {b.AvailableTypeIds}");
        }

        static async Task SearchBooks(ILibraryFacade f)
        {
            Console.Write("Enter search term: ");
            var term = Console.ReadLine();
            var books = await f.SearchBooksAsync(term);
            foreach (var b in books)
                Console.WriteLine($"{b.Id} | {b.Title} | {b.Author}");
        }

        static async Task FilterByType(ILibraryFacade f)
        {
            // 1) Виводимо перелік типів (щоб користувач вибрав)
            Console.WriteLine("Select type:");
            Console.WriteLine("1) Paper");
            Console.WriteLine("2) Audio");
            Console.WriteLine("3) Electronic");
            Console.Write("Choice: ");

            var typeChoice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            int typeId;
            switch (typeChoice)
            {
                case "1":
                    typeId = (int)BookType.Paper;
                    break;
                case "2":
                    typeId = (int)BookType.Audio;
                    break;
                case "3":
                    typeId = (int)BookType.Electronic;
                    break;
                default:
                    Console.WriteLine("Невірний тип. Повертаємось у меню.\n");
                    return;
            }

            // 2) Викликаємо фасад, щоб отримати книги за обраним типом
            var filtered = await f.ReadBooksByTypeAsync(typeId);

            // 3) Якщо нічого не знайдено, повідомляємо
            if (filtered == null || filtered.Count == 0)
            {
                Console.WriteLine("Немає книг із таким типом.\n");
                return;
            }

            // 4) Виводимо знайдені книги
            Console.WriteLine($"=== Books of Type {(BookType)typeId} ===");
            foreach (var b in filtered)
            {
                Console.WriteLine(
                    $"{b.Id} | {b.Title} | {b.Author} | " +
                    $"InitCopies: {b.InitialCopies} | " +
                    $"AvailCopies: {b.AvailableCopies} | " +
                    $"Types: {string.Join(", ", b.AvailableTypeIds)}"
                );
            }
            Console.WriteLine();

            Console.WriteLine("Press any key...");
            Console.ReadKey();
            Console.WriteLine();
        }

        static async Task FilterByGenre(ILibraryFacade f)
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

        static async Task<UserDto> Register(ILibraryFacade f)
        {
            var dto = new UserDto();
            Console.Write("FirstName: "); dto.FirstName = Console.ReadLine();
            Console.Write("LastName: "); dto.LastName = Console.ReadLine();
            Console.Write("Email: "); dto.Email = Console.ReadLine();
            Console.Write("Password: "); dto.Password = Console.ReadLine();
            return await f.RegisterUserAsync(dto);
        }

        static async Task<UserDto> Login(ILibraryFacade f)
        {
            Console.Write("Email: "); var email = Console.ReadLine();
            Console.Write("Password: "); var pwd = Console.ReadLine();
            return await f.AuthenticateAsync(email, pwd);
        }

        private static async Task UpdateProfile(ILibraryFacade f)
        {
            Console.Write("Enter your UserId: ");
            if (!int.TryParse(Console.ReadLine(), out var userId))
            {
                Console.WriteLine("Невірний формат UserId.\n");
                return;
            }

            UserDto user;
            try
            {
                user = await f.GetUserByIdAsync(userId);
            }
            catch (KeyNotFoundException knf)
            {
                Console.WriteLine($"Помилка: {knf.Message}\n");
                return;
            }

            Console.WriteLine("=== Update My Profile ===");
            Console.Write($"Current Email ({user.Email}): ");
            var newEmail = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(newEmail))
                user.Email = newEmail;

            Console.Write($"Current Password (****): ");
            var newPassword = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(newPassword))
                user.Password = newPassword;

            Console.Write($"Current First Name ({user.FirstName}): ");
            var newFirst = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(newFirst))
                user.FirstName = newFirst;

            Console.Write($"Current Last Name ({user.LastName}): ");
            var newLast = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(newLast))
                user.LastName = newLast;


            try
            {
                await f.UpdateUserAsync(user);
                Console.WriteLine("Profile successfully updated.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка під час оновлення: {ex.Message}\n");
            }
        }

        static async Task PlaceOrder(ILibraryFacade f, int userId)
        {
            try
            {
                Console.Write("BookId: ");
                if (!int.TryParse(Console.ReadLine(), out var bookId))
                {
                    Console.WriteLine("Невірний формат BookId.\n");
                    return;
                }

                Console.WriteLine("Select type: 1) Paper 2) Audio 3) Electronic");
                Console.Write("Choice: ");
                var typeChoice = Console.ReadLine()?.Trim();
                BookType orderType;
                switch (typeChoice)
                {
                    case "1":
                        orderType = BookType.Paper;
                        break;
                    case "2":
                        orderType = BookType.Audio;
                        break;
                    case "3":
                        orderType = BookType.Electronic;
                        break;
                    default:
                        Console.WriteLine("Невірний тип.\n");
                        return;
                }

                // Для Audio/Electronic BookCopyId лишається null
                int? bookCopyId = null;

                // Якщо це Paper, BookCopyId ми не запитуємо — сервіс знайде першу вільну копію
                // (тому лишаємо bookCopyId = null)

                Console.Write("UserId: ");
                if (!int.TryParse(Console.ReadLine(), out var enteredUserId))
                {
                    Console.WriteLine("Невірний формат UserId.\n");
                    return;
                }

                var dto = new ActionOrderDto
                {
                    UserId = enteredUserId,
                    BookId = bookId,
                    OrderType = orderType,
                    BookCopyId = bookCopyId
                    // OrderDate призначається всередині сервісу
                };

                await f.CreateOrderAsync(dto);
                Console.WriteLine("Order successfully placed.\n");
            }
            catch (KeyNotFoundException knf)
            {
                Console.WriteLine($"Помилка: {knf.Message}\n");
            }
            catch (InvalidOperationException ioe)
            {
                Console.WriteLine($"Помилка: {ioe.Message}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Сталася помилка під час оформлення замовлення: {ex.Message}\n");
            }
        }

        static async Task CancelOrder(ILibraryFacade f)
        {
            try
            {
                Console.Write("Enter OrderId to cancel: ");
                if (!int.TryParse(Console.ReadLine(), out var orderId))
                {
                    Console.WriteLine("Невірний формат OrderId.\n");
                    return;
                }

                await f.DeleteOrderAsync(orderId);
                Console.WriteLine("Order successfully canceled.\n");
            }
            catch (KeyNotFoundException knf)
            {
                Console.WriteLine($"Помилка: {knf.Message}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Сталася помилка під час скасування замовлення: {ex.Message}\n");
            }
        }

        static async Task ManageBooks(ILibraryFacade f)
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

        static async Task CreateBook(ILibraryFacade f)
        {
            try
            {
                Console.Write("Enter Title: ");
                var title = Console.ReadLine()?.Trim();

                Console.Write("Enter Author: ");
                var author = Console.ReadLine()?.Trim();

                Console.Write("Enter Publisher: ");
                var publisher = Console.ReadLine()?.Trim();

                Console.Write("Enter PublicationYear: ");
                if (!int.TryParse(Console.ReadLine(), out var year))
                {
                    Console.WriteLine("Невірний формат року. Повертаємось у меню.\n");
                    return;
                }

                Console.Write("Enter GenreId: ");
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
                    Console.Write("Enter CopyCount (кількість паперових копій): ");
                    if (!int.TryParse(Console.ReadLine(), out copyCount) || copyCount < 1)
                    {
                        Console.WriteLine("Невірний формат CopyCount. Повертаємось у меню.\n");
                        return;
                    }
                }

                // Формуємо DTO
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

                // Створюємо книгу через фасад
                await f.CreateBookAsync(dto);

                // Щоби переконатися, що в BookDto.InitialAvailableCopies дійсно збереглося copyCount,
                // відразу дістанемо лише що створену книгу (за останнім Id).
                var allBooks = await f.GetAllBooksAsync();
                var lastBook = allBooks.OrderBy(b => b.Id).LastOrDefault();

                Console.WriteLine("\n=== Created Book ===");
                Console.WriteLine($"Id: {lastBook.Id}");
                Console.WriteLine($"Title: {lastBook.Title}");
                Console.WriteLine($"Author: {lastBook.Author}");
                Console.WriteLine($"Publisher: {lastBook.Publisher}");
                Console.WriteLine($"PublicationYear: {lastBook.PublicationYear}");
                Console.WriteLine($"InitialAvailableCopies: {lastBook.InitialCopies}");
                Console.WriteLine($"AvailableTypeIds: {string.Join(", ", lastBook.AvailableTypeIds)}");
                Console.WriteLine("====================\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при створенні книги: {ex.Message}\n");
            }
        }

        static async Task UpdateBook(ILibraryFacade f)
        {
            var books = await f.GetAllBooksAsync();
            Console.WriteLine("=== Список усіх книг ===");
            foreach (var b in books)
            {
                Console.WriteLine(
                    $"{b.Id} | {b.Title} | {b.Author} | {b.Publisher} | " +
                    $"InitCopies: {b.InitialCopies} | " +
                    $"Types: {string.Join(", ", b.AvailableTypeIds)}"
                );
            }
            Console.WriteLine();

            try
            {
                Console.Write("Enter Book Id to update: ");
                if (!int.TryParse(Console.ReadLine(), out var bookId))
                {
                    Console.WriteLine("Невірний формат Id. Повертаємось у меню.\n");
                    return;
                }

                // Завантажуємо цю книгу, щоби показати поточні поля
                var existing = await f.GetBookByIdAsync(bookId);
                if (existing == null)
                {
                    Console.WriteLine($"Книга з Id = {bookId} не знайдена.\n");
                    return;
                }

                Console.WriteLine($"Current Title: {existing.Title}");
                Console.WriteLine($"Current Author: {existing.Author}");
                Console.WriteLine($"Current Publisher: {existing.Publisher}");
                Console.WriteLine($"Current PublicationYear: {existing.PublicationYear}");
                Console.WriteLine($"Current GenreId: {existing.GenreId}");
                Console.WriteLine($"Current InitialAvailableCopies: {existing.InitialCopies}");
                Console.WriteLine($"Current AvailableTypeIds: {string.Join(", ", existing.AvailableTypeIds)}");
                Console.WriteLine("-------------------------------------------------------");

                // Вводимо нові дані
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

                var dto = new ActionBookDto
                {
                    Title = title,
                    Author = author,
                    Publisher = publisher,
                    PublicationYear = year,
                    GenreId = genreId,
                    AvailableTypeIds = typeIds,
                    CopyCount = copyCount  // саме це попадатиме в InitialAvailableCopies
                };

                await f.UpdateBookAsync(bookId, dto);

                // Після оновлення ще раз читаємо цю книгу, щоби показати, що InitialAvailableCopies змінилося
                var updated = await f.GetBookByIdAsync(bookId);
                Console.WriteLine("\n=== Updated Book ===");
                Console.WriteLine($"Id: {updated.Id}");
                Console.WriteLine($"Title: {updated.Title}");
                Console.WriteLine($"Author: {updated.Author}");
                Console.WriteLine($"Publisher: {updated.Publisher}");
                Console.WriteLine($"PublicationYear: {updated.PublicationYear}");
                Console.WriteLine($"InitialAvailableCopies: {updated.InitialCopies}");
                Console.WriteLine($"AvailableTypeIds: {string.Join(", ", updated.AvailableTypeIds)}");
                Console.WriteLine("====================\n");
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
        static async Task DeleteBook(ILibraryFacade facade)
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

        static async Task ManageOrders(ILibraryFacade f)
        {
            Console.WriteLine("1) View All 2) View by id 3) View users 4)Delete");
            var opt = Console.ReadLine();
            switch (opt)
            {
                case "1": await ViewAllOrders(f); break;
                case "2": await ViewOrderById(f); break;
                case "3": await ViewOrdersByUser(f); break;
                case "4": await CancelOrder(f); break;
                case "0": return;
                default: Console.WriteLine("Invalid"); break;
            }
        }

        static async Task ViewAllOrders(ILibraryFacade f)
        {
            var allOrders = await f.ReadAllOrdersAsync();
            if (!allOrders.Any())
            {
                Console.WriteLine("Немає жодного замовлення.\n");
                return;
            }

            Console.WriteLine("=== All Orders ===");
            foreach (var o in allOrders)
            {
                Console.WriteLine($"OrderId: {o.Id} | UserId: {o.UserId} | BookId: {o.BookId} | " +
                                  $"OrderType: {o.OrderType} | BookCopyId: {o.BookCopyId} | Date: {o.OrderDate}");
            }
            Console.WriteLine();
        }

        static async Task ViewOrderById(ILibraryFacade f)
        {
            Console.Write("Enter OrderId: ");
            if (!int.TryParse(Console.ReadLine(), out var orderId))
            {
                Console.WriteLine("Невірний формат OrderId.\n");
                return;
            }

            try
            {
                var o = await f.ReadOrderByIdAsync(orderId);
                Console.WriteLine("=== Order Details ===");
                Console.WriteLine($"OrderId:   {o.Id}");
                Console.WriteLine($"UserId:    {o.UserId}");
                Console.WriteLine($"BookId:    {o.BookId}");
                Console.WriteLine($"OrderType: {o.OrderType}");
                Console.WriteLine($"BookCopyId:{o.BookCopyId}");
                Console.WriteLine($"OrderDate: {o.OrderDate}");
                Console.WriteLine("======================\n");
            }
            catch (KeyNotFoundException knf)
            {
                Console.WriteLine($"Помилка: {knf.Message}\n");
            }
        }

        static async Task ViewOrdersByUser(ILibraryFacade f)
        {
            Console.Write("Enter UserId: ");
            if (!int.TryParse(Console.ReadLine(), out var userId))
            {
                Console.WriteLine("Невірний формат UserId.\n");
                return;
            }

            var orders = await f.ReadOrdersByUserAsync(userId);
            if (!orders.Any())
            {
                Console.WriteLine($"Користувач з Id = {userId} не має замовлень.\n");
                return;
            }

            Console.WriteLine($"=== Orders of UserId {userId} ===");
            foreach (var o in orders)
            {
                Console.WriteLine($"OrderId: {o.Id} | BookId: {o.BookId} | " +
                                  $"OrderType: {o.OrderType} | BookCopyId: {o.BookCopyId} | Date: {o.OrderDate}");
            }
            Console.WriteLine();
        }

        private static async Task ManageGenres(ILibraryFacade f)
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

        private static async Task CreateGenre(ILibraryFacade f)
        {
            Console.Write("Назва жанру: "); var name = Console.ReadLine();
            await f.CreateGenreAsync(new GenreDto { Name = name });
            Console.WriteLine("Жанр додано.");
        }

        private static async Task UpdateGenre(ILibraryFacade f)
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

        private static async Task DeleteGenre(ILibraryFacade f)
        {
            Console.Write("Genre ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid ID"); return; }
            await f.DeleteGenreAsync(id);
            Console.WriteLine("Жанр видалено.");
        }
    }
}
