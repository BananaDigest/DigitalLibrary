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
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDto>> ReadAllAsync()
        {
            var list = await _uow.Orders.ReadAllAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(list);
        }

        public async Task<OrderDto> ReadByIdAsync(int id)
        {
            var order = await _uow.Orders.ReadByIdAsync(id)
                ?? throw new KeyNotFoundException($"Order {id} not found");
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderDto>> ReadByUserAsync(int userId)
        {
            var list = await _uow.Orders.FindAsync(o => o.UserId == userId);
            return _mapper.Map<IEnumerable<OrderDto>>(list);
        }

        public async Task CreateAsync(ActionOrderDto dto)
        {
            var entity = _mapper.Map<Order>(dto);
            await _uow.Orders.CreateAsync(entity);
            await _uow.CommitAsync();
        }

        public async Task UpdateAsync(ActionOrderDto dto)
        {
            var existing = await _uow.Orders.ReadByIdAsync(dto.Id)
                ?? throw new KeyNotFoundException($"Order {dto.Id} not found");
            _mapper.Map(dto, existing);
            _uow.Orders.Update(existing);
            await _uow.CommitAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _uow.Orders.ReadByIdAsync(id)
                ?? throw new KeyNotFoundException($"Order {id} not found");
            _uow.Orders.Delete(existing);
            await _uow.CommitAsync();
        }
    }
}
