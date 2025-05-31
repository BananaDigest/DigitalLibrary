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
            CreateMap<ActionBookDto, Book>();
            CreateMap<UserDto, User>();

            // 1) Щоби Book → BookDto було заповнено InitialAvailableCopies:
            CreateMap<Book, BookDto>()
    .ForMember(dest => dest.AvailableTypeIds,
               opt => opt.MapFrom(src => src.AvailableTypes.Select(at => at.Id).ToList()))
    .ForMember(dest => dest.InitialCopies,
               opt => opt.MapFrom(src => src.InitialCopies))
    .ForMember(dest => dest.AvailableCopies,
               opt => opt.MapFrom(src => src.AvailableCopies));

            // 2) Щоби ActionBookDto → Book (створення/оновлення) встановлювало Book.InitialAvailableCopies = dto.CopyCount:
            CreateMap<ActionBookDto, Book>()
    .ForMember(dest => dest.AvailableTypes, opt => opt.Ignore())
    .ForMember(dest => dest.Copies, opt => opt.Ignore())
    .ForMember(dest => dest.InitialCopies,
               opt => opt.MapFrom(src => src.CopyCount))
    .ForMember(dest => dest.AvailableCopies,
               opt => opt.MapFrom(src => src.CopyCount));


        }
    }
}
