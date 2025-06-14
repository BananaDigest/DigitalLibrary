﻿using DAL.Context;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly DigitalLibraryContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(DigitalLibraryContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task<TEntity> ReadByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> ReadAllAsync()
        {
            if (typeof(TEntity) == typeof(Book))
            {
                return (IEnumerable<TEntity>)await _context.Books
                    .Include(b => b.Genre)
                    .ToListAsync();
            }

            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task CreateAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public IQueryable<Book> ReadAll()
        {
            return _context.Set<Book>();
        }
        public IQueryable<Order> ReadAllOrder()
        {
            return _context.Set<Order>();
        }
        public IQueryable<User> ReadAllUser()
        {
            return _context.Set<User>();
        }

    }
}
