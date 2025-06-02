using BLL.DTOs;
using AutoMapper;
using API.Models;

namespace API.Mapping
{
    public class AutoMapperProfiles:Profile
    {
        public AutoMapperProfiles()
        {
            // ========== User ==========
            // Мапінг із UserActionModel (реєстрація/оновлення) → UserDto
            CreateMap<UserActionModel, UserDto>();
            // Мапінг із UserDto → UserViewModel (для виводу)
            CreateMap<UserDto, UserViewModel>()
                // PasswordHash і Role потрапляють у DTO, але не виводяться в UserViewModel
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            // ========== Book ==========
            // Створення/оновлення книги: CreateBookModel → ActionBookDto
            CreateMap<CreateBookModel, ActionBookDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author))
                .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.Publisher))
                .ForMember(dest => dest.PublicationYear, opt => opt.MapFrom(src => src.PublicationYear))
                .ForMember(dest => dest.GenreId, opt => opt.MapFrom(src => src.GenreId))
                .ForMember(dest => dest.AvailableTypeIds, opt => opt.MapFrom(src => src.AvailableTypeIds))
                .ForMember(dest => dest.CopyCount, opt => opt.MapFrom(src => src.CopyCount))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            // Вивід книги: BookDto → BookViewModel
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

            // ========== Genre ==========
            // Створення/оновлення жанру: GenreViewModel → GenreDto
            CreateMap<GenreViewModel, GenreDto>();
            // Вивід жанру: GenreDto → GenreViewModel
            CreateMap<GenreDto, GenreViewModel>();

            // ========== Order ==========
            // Створення замовлення: CreateOrderModel → ActionOrderDto
            CreateMap<CreateOrderModel, ActionOrderDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId))
                .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => src.OrderType));
            // Вивід замовлення: OrderDto → OrderViewModel
            CreateMap<OrderDto, OrderViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId))
                .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => src.OrderType))
                .ForMember(dest => dest.BookCopyId, opt => opt.MapFrom(src => src.BookCopyId))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate));

            // ========== BookType ==========
            // Вивід типу книги: BookTypeDto → BookTypeViewModel
            CreateMap<BookTypeDto, BookTypeViewModel>();
        }
    }
}
