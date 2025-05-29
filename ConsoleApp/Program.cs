// Program.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using BLL.Facade;
using BLL.Interfaces;
using BLL.Repositories;
using BLL.Factory;
using BLL.Strategy;
using DAL.Context;
using DAL.Repositories;
using DAL.UnitOfWork;
using BLL.DTOs;
using BLL.Services;
using Domain.Enums;
using BLL.Mapping;

namespace ConsoleLibraryApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Получаем строку подключения из аргументов или используем значение по умолчанию
            string connectionString = args.Length > 0
                ? args[0]
                : "Server=(localdb)\\MSSQLLocalDB;Database=DigitalLibrary;Trusted_Connection=True;";

            var services = new ServiceCollection();

            // DbContext и UnitOfWork
            services.AddDbContext<DigitalLibraryContext>(opt =>
                opt.UseSqlServer(connectionString));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Репозиторії для фасаду (якщо потрібно напряму)
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            // AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfiles));

            // BLL сервіси
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IGenreService, GenreService>();
            services.AddScoped<IUserService, UserService>();

            // Фасад
            services.AddScoped<LibraryFacade>();

            var provider = services.BuildServiceProvider();
            var facade = provider.GetRequiredService<LibraryFacade>();

            Console.WriteLine("Using connection string: " + connectionString);
            await RunConsoleAsync(facade);
        }

        static async Task RunConsoleAsync(LibraryFacade f)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Library Console ===");
                Console.WriteLine("1. Add Book");
                Console.WriteLine("2. Find Books");
                Console.WriteLine("3. Delete Book");
                Console.WriteLine("4. Place Order");
                Console.WriteLine("5. Delete Order");
                Console.WriteLine("6. View Available Copies");
                Console.WriteLine("7. Report by Types");
                Console.WriteLine("0. Exit");
                Console.Write("Option: ");
                var opt = Console.ReadLine();

                try
                {
                    switch (opt)
                    {
                        case "1": await AddBook(f); break;
                        case "2": await FindBooks(f); break;
                        case "3": await DeleteBook(f); break;
                        case "4": await PlaceOrder(f); break;
                        case "5": await DeleteOrder(f); break;
                        case "6": await ViewCopies(f); break;
                        case "7": await ReportTypes(f); break;
                        case "0": return;
                        default: Console.WriteLine("Invalid"); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine("Press any key...");
                Console.ReadKey();
            }
        }
        static async Task AddBook(LibraryFacade f)
        {
            var dto = new ActionBookDto();
            Console.Write("Title: "); dto.Title = Console.ReadLine();
            Console.Write("Author: "); dto.Author = Console.ReadLine();
            Console.Write("Publisher: "); dto.Publisher = Console.ReadLine();
            Console.Write("Year: "); dto.PublicationYear = int.Parse(Console.ReadLine());
            Console.Write("Type (paper/audio/electronic): ");
            var type = Console.ReadLine().ToLower();
            dto.AvailableTypes = type switch
            {
                "paper" => BookType.Paper,
                "audio" => BookType.Audio,
                _ => BookType.Electronic
            };
            Console.Write("GenreId: "); dto.GenreId = Guid.Parse(Console.ReadLine());

            var id = await f.CreateBookAsync(dto);
            Console.WriteLine($"Book created: {id}");
        }

        static async Task FindBooks(LibraryFacade f)
        {
            Console.Write("Criterion: "); var crit = Console.ReadLine();
            Console.Write("Filter by (author/title): "); var filter = Console.ReadLine();
            var list = await f.FindBooksAsync(crit, filter);
            foreach (var b in list)
                Console.WriteLine($"{b.Id} | {b.Title} | {b.Author}");
        }

        static async Task DeleteBook(LibraryFacade f)
        {
            Console.Write("BookId: "); var id = Guid.Parse(Console.ReadLine());
            await f.DeleteBookAsync(id);
            Console.WriteLine("Deleted");
        }

        static async Task PlaceOrder(LibraryFacade f)
        {
            var dto = new ActionOrderDto();
            Console.Write("UserId: "); dto.UserId = Guid.Parse(Console.ReadLine());
            Console.Write("BookId: "); dto.BookId = Guid.Parse(Console.ReadLine());
            Console.Write("OrderType (paper/audio/electronic): ");
            var t = Console.ReadLine().ToLower();
            dto.OrderType = t switch
            {
                "paper" => BookType.Paper,
                "audio" => BookType.Audio,
                _ => BookType.Electronic
            };
            Console.Write("BookCopyId (optional): ");
            var copy = Console.ReadLine();
            dto.BookCopyId = string.IsNullOrWhiteSpace(copy) ? null : Guid.Parse(copy);
            await f.CreateOrderAsync(dto);
            Console.WriteLine("Order placed");
        }

        static async Task DeleteOrder(LibraryFacade f)
        {
            Console.Write("OrderId: "); var id = Guid.Parse(Console.ReadLine());
            await f.DeleteOrderAsync(id);
            Console.WriteLine("Order deleted");
        }

        static async Task ViewCopies(LibraryFacade f)
        {
            Console.Write("BookId: "); var id = Guid.Parse(Console.ReadLine());
            var copies = await f.ReadAvailableCopiesAsync(id);
            foreach (var c in copies)
                Console.WriteLine($"Copy {c.CopyNumber}: {(c.IsAvailable ? "Available" : "Taken")}");
        }

        static async Task ReportTypes(LibraryFacade f)
        {
            var report = await f.ReadReportByTypesAsync();
            foreach (var kv in report)
                Console.WriteLine($"{kv.Key}: {kv.Value}");
        }
    }
}