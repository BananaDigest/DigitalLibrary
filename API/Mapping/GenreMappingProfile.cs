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
    public class GenreMappingProfile : Profile
    {
        public GenreMappingProfile()
        {
            // GenreViewModel → GenreDto
            CreateMap<GenreViewModel, GenreDto>();

            // GenreDto → GenreViewModel
            CreateMap<GenreDto, GenreViewModel>();
        }
    }
}
