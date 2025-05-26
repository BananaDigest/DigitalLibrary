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
            CreateMap<User, UserRegistrationDto>();
            CreateMap<Order, OrderDto>();
            CreateMap<BookCopy, BookCopyDto>();

            // DTO -> Domain
            CreateMap<CreateBookDto, Book>();
            CreateMap<UserRegistrationDto, User>();

            // При необхідності додати кастомні налаштування:
            // .ForMember(dest => dest.Property, opt => opt.MapFrom(src => src.OtherProperty));
        }
    }
}
