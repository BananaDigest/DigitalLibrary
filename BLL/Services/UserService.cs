using AutoMapper;
using BLL.DTOs;
using BLL.Interfaces;
using DAL.UnitOfWork;
using Domain.Entities;
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

        public async Task RegisterAsync(UserRegistrationDto dto)
        {
            var user = _mapper.Map<User>(dto);
            await _uow.Users.AddAsync(user);
            await _uow.CommitAsync();
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            var user = await _uow.Users.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"User {id} not found");
            return _mapper.Map<UserDto>(user);
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _uow.Users.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"User {id} not found");
            _uow.Users.Remove(existing);
            await _uow.CommitAsync();
        }
    }
}
