using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Strategy
{
    public class BookFilterContext
    {
        private IBookFilterStrategy _strategy;
        public void SetStrategy(IBookFilterStrategy strategy) => _strategy = strategy;
        public Task<IEnumerable<Book>> ExecuteFilter(string criterion)
            => _strategy.FilterAsync(criterion);
    }
}
