using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IUserService
    {
        Task RegisterAsync(UserDto dto);
        Task<UserDto> GetByIdAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}
