﻿using BLL.DTOs;
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
        Task<UserDto> ReadByIdAsync(int id);
        Task<UserDto> AuthenticateAsync(string email, string password);
        Task DeleteAsync(int id);
        Task<IEnumerable<UserDto>> ReadAllUsersAsync();
        Task UpdateUserAsync(UserDto dto);
    }
}
