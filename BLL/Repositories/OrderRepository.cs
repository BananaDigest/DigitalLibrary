using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DAL.Context;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DigitalLibraryContext _context;
        private readonly DbSet<Order> _dbSet;

        public OrderRepository(DigitalLibraryContext context)
        {
            _context = context;
            _dbSet = _context.Set<Order>();
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
            => await _dbSet.Include(o => o.Book).ToListAsync();

        public async Task<Order> GetByIdAsync(Guid id)
            => await _dbSet.Include(o => o.Book)
                           .FirstOrDefaultAsync(o => o.Id == id);

        public async Task<IEnumerable<Order>> FindAsync(Expression<Func<Order, bool>> predicate)
            => await _dbSet.Where(predicate).Include(o => o.Book).ToListAsync();

        public async Task AddAsync(Order order)
        {
            await _dbSet.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _dbSet.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
            => await _dbSet.AnyAsync(o => o.Id == id);
    }
}
