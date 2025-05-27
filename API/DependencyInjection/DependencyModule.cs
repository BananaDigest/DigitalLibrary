using API.Controllers;
using BLL.Facade;
using BLL.Interfaces;
using BLL.Repositories;
using BLL.Services;
using DAL.Repositories;
using DAL.Context;
using System.Reflection;
using Autofac;

namespace API.DependencyInjection
{
    public class DependencyModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // --- Репозиторії ---
            builder.RegisterType<BookRepository>()
                   .As<IBookRepository>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<OrderRepository>()
                   .As<IOrderRepository>()
                   .InstancePerLifetimeScope();

            // --- Сервіси ---
            // Методи у сервісах мають назви Create, Read, Delete
            builder.RegisterType<BookService>()
                   .As<IBookService>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<OrderService>()
                   .As<IOrderService>()
                   .InstancePerLifetimeScope();
        }
    }
}
