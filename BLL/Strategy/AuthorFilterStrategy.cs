using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Repositories;
using Domain.Entities;
using BLL.Strategy;

namespace BLL.Strategy
{
    public class AuthorFilterStrategy : IBookFilterStrategy
    {
        private readonly IGenericRepository<Book> _repository;

        public AuthorFilterStrategy(IGenericRepository<Book> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Book>> FilterAsync(string criterion)
        {
            var books = await _repository.ReadAllAsync();
            return books
                .Where(b => b.Author != null &&
                            b.Author.Contains(criterion, StringComparison.OrdinalIgnoreCase));
        }
    }
}
