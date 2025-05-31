using AutoMapper;
using BLL.DTOs;
using BLL.Interfaces;
using DAL.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class BookTypeService : IBookTypeService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public BookTypeService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<List<BookTypeDto>> GetAllBookTypesAsync()
        {
            var entities = await _uow.BookTypes.ReadAllAsync();
            return _mapper.Map<List<BookTypeDto>>(entities.ToList());
        }
    }
}
