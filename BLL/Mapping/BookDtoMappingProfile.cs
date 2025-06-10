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
    public class BookDtoMappingProfile : Profile
    {
        public BookDtoMappingProfile()
        {
            // Book -> BookDto
            CreateMap<Book, BookDto>()
                .ForMember(dest => dest.AvailableTypeIds,
                           opt => opt.MapFrom(src => src.AvailableTypes.Select(at => at.Id).ToList()))
                .ForMember(dest => dest.InitialCopies,
                           opt => opt.MapFrom(src => src.InitialCopies))
                .ForMember(dest => dest.AvailableCopies,
                           opt => opt.MapFrom(src => src.AvailableCopies))
                .ForMember(dest => dest.DownloadCount,
                           opt => opt.MapFrom(src => src.DownloadCount))
                .ForMember(dest => dest.ListenCount,
                           opt => opt.MapFrom(src => src.ListenCount))
                .ForMember(dest => dest.Description,
                           opt => opt.MapFrom(src => src.Description));

            // ActionBookDto -> Book
            CreateMap<ActionBookDto, Book>()
                .ForMember(dest => dest.AvailableTypes, opt => opt.Ignore())
                .ForMember(dest => dest.Copies, opt => opt.Ignore())
                .ForMember(dest => dest.InitialCopies,
                           opt => opt.MapFrom(src => src.CopyCount))
                .ForMember(dest => dest.AvailableCopies,
                           opt => opt.MapFrom(src => src.CopyCount))
                .ForMember(dest => dest.Description,
                           opt => opt.MapFrom(src => src.Description));
        }
    }
}
