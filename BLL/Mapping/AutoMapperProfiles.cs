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
            CreateMap<GenreDto, Genre>()
            .ReverseMap();
            CreateMap<BookTypeEntity, BookTypeDto>()
                .ReverseMap();


            // DTO -> Domain
            CreateMap<ActionBookDto, Book>()
                .ForMember(dest => dest.AvailableTypes,
                    opt => opt.MapFrom((src, dest, _, ctx) =>
                    src.AvailableTypeIds
                       .Select(id => new BookTypeEntity { Id = id })
                       .ToList()));
            CreateMap<UserDto, User>();

            // При необхідності додати кастомні налаштування:
            // .ForMember(dest => dest.Property, opt => opt.MapFrom(src => src.OtherProperty));
        }
    }
}
