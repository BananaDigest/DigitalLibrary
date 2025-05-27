using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Repositories
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> ReadAllAsync();
        Task<Book> ReadByIdAsync(Guid id);
        Task<IEnumerable<Book>> FindAsync(Expression<Func<Book, bool>> predicate);
        Task CreateAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
