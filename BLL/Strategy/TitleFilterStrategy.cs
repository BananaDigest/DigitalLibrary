using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using BLL.Strategy;
using DAL.Repositories;

namespace BLL.Strategy
{
    public class TitleFilterStrategy : IBookFilterStrategy
    {
        private readonly IGenericRepository<Book> _repository;

        public TitleFilterStrategy(IGenericRepository<Book> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Book>> FilterAsync(string criterion)
        {
            var books = await _repository.ReadAllAsync();
            return books
                .Where(b => b.Title != null &&
                            b.Title.Contains(criterion, StringComparison.OrdinalIgnoreCase));
        }
    }
}
