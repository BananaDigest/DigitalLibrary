using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Strategy
{
    public interface IBookFilterStrategy
    {
        Task<IEnumerable<Book>> FilterAsync(string criterion);
    }
}
