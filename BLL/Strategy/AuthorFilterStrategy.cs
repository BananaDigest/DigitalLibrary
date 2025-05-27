using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Strategy
{
    public class AuthorFilterStrategy : IBookFilterStrategy
    {
        private readonly IBookRepository _repository;
        public AuthorFilterStrategy(IBookRepository repository) => _repository = repository;
        public async Task<IEnumerable<Book>> FilterAsync(string criterion)
        {
            var all = await _repository.GetAllAsync();
            return all.Where(b => b.Author.Contains(criterion, StringComparison.OrdinalIgnoreCase));
        }
    }
}
