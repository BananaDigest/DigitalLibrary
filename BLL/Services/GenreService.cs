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
    public class GenreService : IGenreService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GenreService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GenreDto>> GetAllAsync()
        {
            var entities = await _uow.Genres.GetAllAsync();
            return _mapper.Map<IEnumerable<GenreDto>>(entities);
        }

        public async Task<GenreDto> GetByIdAsync(Guid id)
        {
            var genre = await _uow.Genres.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Genre {id} not found");
            return _mapper.Map<GenreDto>(genre);
        }

        public async Task CreateAsync(GenreDto dto)
        {
            var entity = _mapper.Map<Genre>(dto);
            await _uow.Genres.AddAsync(entity);
            await _uow.CommitAsync();
        }

        public async Task UpdateAsync(GenreDto dto)
        {
            var existing = await _uow.Genres.GetByIdAsync(dto.Id)
                ?? throw new KeyNotFoundException($"Genre {dto.Id} not found");
            _mapper.Map(dto, existing);
            _uow.Genres.Update(existing);
            await _uow.CommitAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _uow.Genres.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Genre {id} not found");
            _uow.Genres.Remove(existing);
            await _uow.CommitAsync();
        }
    }
}
