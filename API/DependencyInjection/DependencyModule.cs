using API.Controllers;
using BLL.Facade;
using BLL.Interfaces;
using BLL.Services;
using DAL.Repositories;
using DAL.Context;
using System.Reflection;
using Autofac;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BLL.Factory;
using BLL.Strategy;
using API.Mapping;
using BLL.Mapping;

namespace API.DependencyInjection
{
    public class DependencyModule : Autofac.Module
    {
        private readonly IConfiguration _configuration;

        public DependencyModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // 1. DbContext
            builder.Register(ctx =>
            {
                var options = new DbContextOptionsBuilder<DigitalLibraryContext>()
                    .UseSqlServer(_configuration.GetConnectionString("DefaultConnection"))
                    .Options;
                return new DigitalLibraryContext(options);
            })
            .AsSelf()
            .InstancePerLifetimeScope();

            // 2. UnitOfWork
            builder.RegisterType<UnitOfWork>()
                   .As<IUnitOfWork>()
                   .InstancePerLifetimeScope();

            // 3. BLL‐сервіси
            builder.RegisterType<BookService>()
                   .As<IBookService>()
                   .InstancePerLifetimeScope();
            builder.RegisterType<OrderService>()
                   .As<IOrderService>()
                   .InstancePerLifetimeScope();
            builder.RegisterType<GenreService>()
                   .As<IGenreService>()
                   .InstancePerLifetimeScope();
            builder.RegisterType<UserService>()
                   .As<IUserService>()
                   .InstancePerLifetimeScope();
            builder.RegisterType<BookTypeService>()
                   .As<IBookTypeService>()
                   .InstancePerLifetimeScope();

            // 4. Фабрика для Book
            builder.RegisterType<BookFactory>()
                   .As<IBookFactory>()
                   .InstancePerLifetimeScope();

            // 5. Стратегії фільтрації
            builder.RegisterType<AuthorFilterStrategy>()
                   .As<IBookFilterStrategy>()
                   .Named<IBookFilterStrategy>("author")
                   .InstancePerLifetimeScope();
            builder.RegisterType<TitleFilterStrategy>()
                   .As<IBookFilterStrategy>()
                   .Named<IBookFilterStrategy>("title")
                   .InstancePerLifetimeScope();

            // 6. Контекст для вибору стратегії
            builder.RegisterType<BookFilterContext>()
                   .AsSelf()
                   .InstancePerLifetimeScope();

            // 7. Фасад (Facade)
            builder.RegisterType<LibraryFacade>()
                   .As<ILibraryFacade>()
                   .InstancePerLifetimeScope();

            // 8. AutoMapper: додаємо всі профілі із двох збірок
            builder.Register(context =>
            {
                var cfg = new MapperConfiguration(cfg =>
                {
                    // скануємо всю збірку API.Mapping
                    cfg.AddMaps(typeof(UserMappingProfile).Assembly);
                    // скануємо всю збірку BLL.Mapping
                    cfg.AddMaps(typeof(UserDtoMappingProfile).Assembly);
                });
                return cfg;
            })
            .AsSelf()
            .SingleInstance();

            builder.Register(c =>
            {
                var ctx = c.Resolve<IComponentContext>();
                var config = ctx.Resolve<MapperConfiguration>();
                return config.CreateMapper(ctx.Resolve);
            })
            .As<IMapper>()
            .InstancePerLifetimeScope();
        }
    }
}
