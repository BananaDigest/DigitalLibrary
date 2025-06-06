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
    public class OrderDtoMappingProfile : Profile
    {
        public OrderDtoMappingProfile()
        {
            // Order → OrderDto
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId))
                .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => src.OrderTypeId))
                .ForMember(dst => dst.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.BookCopyId, opt => opt.MapFrom(src => src.BookCopyId))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate));

            // ActionOrderDto → Order
            CreateMap<ActionOrderDto, Order>()
                .ForMember(dest => dest.UserId,
                           opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.BookId,
                           opt => opt.MapFrom(src => src.BookId))
                .ForMember(dest => dest.OrderTypeId,
                           opt => opt.MapFrom(src => (int)src.OrderType))
                .ForMember(dest => dest.OrderType, opt => opt.Ignore())
                .ForMember(dst => dst.OrderTypeId, opt => opt.MapFrom(src => (int)src.OrderType))
                .ForMember(dest => dest.BookCopyId,
                           opt => opt.MapFrom(src => src.BookCopyId))
                .ForMember(dest => dest.OrderDate, opt => opt.Ignore());
        }
    }
}
