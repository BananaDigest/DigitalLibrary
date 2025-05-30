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
            // DbContext Registration
            builder.Register(c =>
            {
                var options = new DbContextOptionsBuilder<DigitalLibraryContext>()
                    .UseSqlServer(_configuration.GetConnectionString("DefaultConnection"))
                    .Options;
                return new DigitalLibraryContext(options);
            })
            .AsSelf()
            .InstancePerLifetimeScope();

            // Unit of Work
            builder.RegisterType<UnitOfWork>()
                   .As<IUnitOfWork>()
                   .InstancePerLifetimeScope();

            // BLL Services
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

            // Facade
            builder.RegisterType<LibraryFacade>()
                   .InstancePerLifetimeScope();
        }
    }
}
