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
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // UserActionModel -> UserDto
            CreateMap<UserActionModel, UserDto>();

            // UserDto -> UserViewModel
            CreateMap<UserDto, UserViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
        }
    }
}
