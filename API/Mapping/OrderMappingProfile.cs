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
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            // CreateOrderModel → ActionOrderDto
            CreateMap<CreateOrderModel, ActionOrderDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId))
                .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => src.OrderType));

            // OrderDto → OrderViewModel
            CreateMap<OrderDto, OrderViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId))
                .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => src.OrderType))
                .ForMember(dest => dest.BookCopyId, opt => opt.MapFrom(src => src.BookCopyId))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate));
        }
    }
}
