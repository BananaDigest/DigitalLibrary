using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.Interfaces
{

    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllAsync();
        Task<OrderDto> GetByIdAsync(Guid id);
        Task<IEnumerable<OrderDto>> GetByUserAsync(Guid userId);
        Task CreateAsync(ActionOrderDto dto);
        Task UpdateAsync(ActionOrderDto dto);
        Task DeleteAsync(Guid id);
    }
}
