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
    public class BookMappingProfile : Profile
    {
        public BookMappingProfile()
        {
            // CreateBookModel -> ActionBookDto
            CreateMap<CreateBookModel, ActionBookDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author))
                .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.Publisher))
                .ForMember(dest => dest.PublicationYear, opt => opt.MapFrom(src => src.PublicationYear))
                .ForMember(dest => dest.GenreId, opt => opt.MapFrom(src => src.GenreId))
                .ForMember(dest => dest.AvailableTypeIds, opt => opt.MapFrom(src => src.AvailableTypeIds))
                .ForMember(dest => dest.CopyCount, opt => opt.MapFrom(src => src.CopyCount))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // BookDto -> BookViewModel
            CreateMap<BookDto, BookViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author))
                .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.Publisher))
                .ForMember(dest => dest.PublicationYear, opt => opt.MapFrom(src => src.PublicationYear))
                .ForMember(dest => dest.GenreId, opt => opt.MapFrom(src => src.Genre.Id))
                .ForMember(dest => dest.GenreName, opt => opt.MapFrom(src => src.Genre.Name))
                .ForMember(dest => dest.AvailableTypeIds, opt => opt.MapFrom(src => src.AvailableTypeIds))
                .ForMember(dest => dest.InitialCopies, opt => opt.MapFrom(src => src.InitialCopies))
                .ForMember(dest => dest.AvailableCopies, opt => opt.MapFrom(src => src.AvailableCopies))
                .ForMember(dest => dest.DownloadCount, opt => opt.MapFrom(src => src.DownloadCount))
                .ForMember(dest => dest.ListenCount, opt => opt.MapFrom(src => src.ListenCount))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
        }
    }
}
