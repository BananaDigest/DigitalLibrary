using BLL.DTOs;
using Domain.Entities;
using AutoMapper;

namespace BLL.Mapping
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Domain -> DTO
            CreateMap<Book, BookDto>();
            CreateMap<Genre, GenreDto>();
            CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email,
               opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Password,
               opt => opt.MapFrom(src => src.Password));
            CreateMap<Order, OrderDto>();
            CreateMap<BookCopy, BookCopyDto>();


            // DTO -> Domain
            CreateMap<ActionBookDto, Book>();
            CreateMap<UserDto, User>();

            // При необхідності додати кастомні налаштування:
            // .ForMember(dest => dest.Property, opt => opt.MapFrom(src => src.OtherProperty));
        }
    }
}
