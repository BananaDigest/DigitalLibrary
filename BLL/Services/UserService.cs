using AutoMapper;
using BLL.DTOs;
using BLL.Interfaces;
using DAL.UnitOfWork;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task RegisterAsync(UserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            await _uow.Users.CreateAsync(user);
            //await _uow.CommitAsync();
            try
            {
                await _uow.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("Збереження не вдалося: " + ex.InnerException?.Message);
                throw;  // пробросити далі або обробити за потреби
            }
        }

        public async Task<UserDto> ReadByIdAsync(int id)
        {
            var user = await _uow.Users.ReadByIdAsync(id)
                ?? throw new KeyNotFoundException($"User {id} not found");
            return _mapper.Map<UserDto>(user);
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _uow.Users.ReadByIdAsync(id)
                ?? throw new KeyNotFoundException($"User {id} not found");
            _uow.Users.Delete(existing);
            await _uow.CommitAsync();
        }
        public async Task<UserDto> AuthenticateAsync(string email, string password)
        {
            // 1) Знаходимо по email
            var allUsers = await _uow.Users.ReadAllAsync();
            var user = allUsers.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                ?? throw new UnauthorizedAccessException("Невірний email або пароль");

            // 2) Простий Plain-text порівняння (для демонстрації)
            if (user.Password != password)
                throw new UnauthorizedAccessException("Невірний email або пароль");

            return _mapper.Map<UserDto>(user);
        }
    }
}
