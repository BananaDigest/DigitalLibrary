using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity> ReadByIdAsync(Guid id);
        Task<IEnumerable<TEntity>> ReadAllAsync();
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task CreateAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
