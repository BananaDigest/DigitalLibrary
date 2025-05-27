using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BLL.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using DAL.Context;

namespace DAL.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly DigitalLibraryContext _context;
        private readonly DbSet<Book> _dbSet;

        public BookRepository(DigitalLibraryContext context)
        {
            _context = context;
            _dbSet = _context.Set<Book>();
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _dbSet.Include(b => b.Copies).ToListAsync();
        }

        public async Task<Book> GetByIdAsync(Guid id)
        {
            return await _dbSet.Include(b => b.Copies).FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Book>> FindAsync(Expression<Func<Book, bool>> predicate)
        {
            return await _dbSet.Where(predicate).Include(b => b.Copies).ToListAsync();
        }

        public async Task AddAsync(Book book)
        {
            await _dbSet.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            _dbSet.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var book = await _dbSet.FindAsync(id);
            if (book != null)
            {
                _dbSet.Remove(book);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbSet.AnyAsync(b => b.Id == id);
        }
    }
}
