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

namespace ConsoleLibraryApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Налаштування DI
            var services = new ServiceCollection();

            // DbContext та UnitOfWork
            services.AddDbContext<DigitalLibraryContext>(opt =>
                opt.UseSqlServer("YourConnectionString"));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

            // BLL сервіси
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IGenreService, GenreService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IUserService, UserService>();

            // Фасад
            services.AddScoped<LibraryFacade>();

            var provider = services.BuildServiceProvider();
            var facade = provider.GetRequiredService<LibraryFacade>();

            await RunConsoleAsync(facade);
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
                Console.WriteLine($"{b.Id} | {b.Title} | {b.Author} | {b.Publisher} | Types: {b.AvailableTypes}");
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
            var opt = Console.ReadLine();
            var type = opt switch
            {
                "1" => BookType.Paper,
                "2" => BookType.Audio,
                _ => BookType.Electronic
            };
            var books = await f.FilterBooksByTypeAsync(type);
            foreach (var b in books)
                Console.WriteLine($"{b.Id} | {b.Title} | {b.AvailableTypes}");
        }

        static async Task FilterByGenre(LibraryFacade f)
        {
            var genres = await f.GetAllGenresAsync();
            Console.WriteLine("Available genres:");
            foreach (var g in genres)
                Console.WriteLine($"{g.Id} - {g.Name}");
            Console.Write("Enter genre ID: ");
            var id = Guid.Parse(Console.ReadLine());
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

        static async Task PlaceOrder(LibraryFacade f, Guid userId)
        {
            Console.Write("BookId: "); var bid = Guid.Parse(Console.ReadLine());
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
            // Реалізувати CRUD через f.CreateBookAsync, UpdateBookAsync, DeleteBookAsync
        }

        static async Task ManageOrders(LibraryFacade f)
        {
            Console.WriteLine("1) View All 2) Delete");
            var opt = Console.ReadLine();
            // Реалізувати перегляд через f.GetAllOrdersAsync та видалення через f.DeleteOrderAsync
        }
    }
}
