using BLL.DTOs;
using Domain.Entities;
using AutoMapper;

namespace BLL.Mapping
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {


            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));
            // Якщо у UserDto є Role, додаємо:
            // .ForMember(dest => dest.Role,     opt => opt.MapFrom(src => src.Role));

            // UserDto → User (ігноруємо зміну ролі)
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Role, opt => opt.Ignore());

            //
            // === Book ↔ BookDto ===
            //
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

            CreateMap<ActionBookDto, Book>()
    .ForMember(dest => dest.AvailableTypes, opt => opt.Ignore())
    .ForMember(dest => dest.Copies, opt => opt.Ignore())
    .ForMember(dest => dest.InitialCopies,
               opt => opt.MapFrom(src => src.CopyCount))
    .ForMember(dest => dest.AvailableCopies,
               opt => opt.MapFrom(src => src.CopyCount))
    .ForMember(dest => dest.Description,
               opt => opt.MapFrom(src => src.Description)); // якщо додаєте Description до ActionBookDto


            //
            // === BookCopy ↔ BookCopyDto ===
            //
            CreateMap<BookCopy, BookCopyDto>().ReverseMap();

            //
            // === Order ↔ OrderDto ===
            //
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Id,
                           opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId,
                           opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.BookId,
                           opt => opt.MapFrom(src => src.BookId))
                .ForMember(dest => dest.OrderType,
                           opt => opt.MapFrom(src => src.OrderType))
                .ForMember(dest => dest.BookCopyId,
                           opt => opt.MapFrom(src => src.BookCopyId))
                .ForMember(dest => dest.OrderDate,
                           opt => opt.MapFrom(src => src.OrderDate));

            CreateMap<ActionOrderDto, Order>()
                .ForMember(dest => dest.UserId,
                           opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.BookId,
                           opt => opt.MapFrom(src => src.BookId))
                .ForMember(dest => dest.OrderType,
                           opt => opt.MapFrom(src => src.OrderType))
                .ForMember(dest => dest.BookCopyId,
                           opt => opt.MapFrom(src => src.BookCopyId))
                // OrderDate буде призначатися безпосередньо в сервісі
                .ForMember(dest => dest.OrderDate,
                           opt => opt.MapFrom(src => src.OrderDate));

            CreateMap<Order, OrderDto>()
                // Прості поля автоматично (Id, UserId, BookId, BookCopyId, OrderDate)
                .ForMember(dest => dest.OrderType,
                           opt => opt.MapFrom(src => src.OrderTypeId));

            //
            // 2) ActionOrderDto → Order
            //
            CreateMap<ActionOrderDto, Order>()
                .ForMember(dest => dest.UserId,
                           opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.BookId,
                           opt => opt.MapFrom(src => src.BookId))
                // Тепер замапимо enum OrderType → ціле BookTypeId:
                .ForMember(dest => dest.OrderTypeId,
                           opt => opt.MapFrom(src => (int)src.OrderType))
                // Ігноруємо навігаційну властивість OrderType:
                .ForMember(dest => dest.OrderType, opt => opt.Ignore())
                .ForMember(dest => dest.BookCopyId,
                           opt => opt.MapFrom(src => src.BookCopyId))
                // Дату будемо встановлювати напряму в сервісі:
                .ForMember(dest => dest.OrderDate, opt => opt.Ignore());
        }
    }
}
