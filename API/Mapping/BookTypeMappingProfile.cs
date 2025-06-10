using API.Models;
using AutoMapper;
using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Mapping
{
    public class BookTypeMappingProfile : Profile
    {
        public BookTypeMappingProfile()
        {
            // BookTypeDto -> BookTypeViewModel
            CreateMap<BookTypeDto, BookTypeViewModel>();
        }
    }
}
