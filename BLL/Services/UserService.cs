using AutoMapper;
using BLL.DTOs;
using BLL.Interfaces;
using DAL.UnitOfWork;
using Domain.Entities;
using Domain.Enums;
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
            user.Role = UserRole.Registered;
            await _uow.Users.CreateAsync(user);
            try
            {
                await _uow.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("Збереження не вдалося: " + ex.InnerException?.Message);
                throw;
            }
        }

        public async Task<IEnumerable<UserDto>> ReadAllUsersAsync()
        {
            var allEntities = await _uow.Users.ReadAllAsync();

            var dtoList = _mapper.Map<List<UserDto>>(allEntities);

            foreach (var dto in dtoList)
            {
                dto.Password = null;
            }

            return dtoList;
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
            //Знаходимо по email
            var allUsers = await _uow.Users.ReadAllAsync();
            var user = allUsers.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                ?? throw new UnauthorizedAccessException("Невірний email");

            if (user.Password != password)
                throw new UnauthorizedAccessException("Невірний пароль");

            return _mapper.Map<UserDto>(user);
        }

        public async Task UpdateUserAsync(UserDto dto)
        {
            var userEntity = await _uow.Users
                .ReadAllUser()   
                .FirstOrDefaultAsync(u => u.Id == dto.Id);

            if (userEntity == null)
                throw new KeyNotFoundException($"User with Id = {dto.Id} not found.");

            _mapper.Map(dto, userEntity);

            _uow.Users.Update(userEntity);

            await _uow.CommitAsync();
        }
    }
}
