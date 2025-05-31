using DAL.Repositories;
using Domain.Entities;
using System.Threading.Tasks;

namespace DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Book> Books { get; }
        IGenericRepository<BookCopy> BookCopies { get; }
        IGenericRepository<Genre> Genres { get; }
        IGenericRepository<User> Users { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<BookTypeEntity> BookTypes { get; }

        Task<int> CommitAsync();
    }
}
