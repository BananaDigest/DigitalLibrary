using AutoMapper;
using BLL.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Mapping
{
    public class BookTypeDtoMappingProfile : Profile
    {
        public BookTypeDtoMappingProfile()
        {
            // BookTypeEntity → BookTypeDto
            CreateMap<BookTypeEntity, BookTypeDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}
