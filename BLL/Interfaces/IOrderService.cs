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
        Task<IEnumerable<OrderDto>> ReadAllAsync();
        Task<OrderDto> ReadByIdAsync(int id);
        Task<IEnumerable<OrderDto>> ReadByUserAsync(int userId);
        Task CreateAsync(ActionOrderDto dto);
        Task UpdateStatusAsync(int orderId);
        Task DeleteAsync(int id, bool isAdmin);
    }
}
