using DAL.Context;
using DAL.Repositories;
using Domain.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DigitalLibraryContext _context;

        public IGenericRepository<Book> Books { get; }
        public IGenericRepository<BookCopy> BookCopies { get; }
        public IGenericRepository<Genre> Genres { get; }
        public IGenericRepository<User> Users { get; }
        public IGenericRepository<Order> Orders { get; }

        public UnitOfWork(DigitalLibraryContext context)
        {
            _context = context;
            Books = new GenericRepository<Book>(_context);
            BookCopies = new GenericRepository<BookCopy>(_context);
            Genres = new GenericRepository<Genre>(_context);
            Users = new GenericRepository<User>(_context);
            Orders = new GenericRepository<Order>(_context);
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
