using AutoMapper;
using BLL.DTOs;
using BLL.Interfaces;
using DAL.UnitOfWork;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDto>> ReadAllAsync()
        {
            var entities = await _uow.Orders
                .ReadAllOrder()                      // IQueryable<Order>
                .Include(o => o.Book)                // навігація, якщо потрібна
                .Include(o => o.BookCopy)            // навігація копії
                .Include(o => o.OrderType)           // навігація типу книги
                .ToListAsync();

            return _mapper.Map<List<OrderDto>>(entities);
        }

        public async Task<OrderDto> ReadByIdAsync(int id)
        {
            var entity = await _uow.Orders
                .ReadAllOrder()
                .Include(o => o.Book)
                .Include(o => o.BookCopy)
                .Include(o => o.OrderType)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (entity == null)
                throw new KeyNotFoundException($"Order with Id = {id} not found.");

            return _mapper.Map<OrderDto>(entity);
        }

        public async Task<IEnumerable<OrderDto>> ReadByUserAsync(int userId)
        {
            var entities = await _uow.Orders
                .ReadAllOrder()
                .Include(o => o.Book)
                .Include(o => o.BookCopy)
                .Include(o => o.OrderType)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return _mapper.Map<List<OrderDto>>(entities);
        }

        public async Task CreateAsync(ActionOrderDto dto)
        {
            // 1) Завантажити книгу з AvailableTypes і Copies
            var bookEntity = await _uow.Books
                .ReadAll()                      // IQueryable<Book>
                .Include(b => b.AvailableTypes) // щоби перевірити, чи доступний потрібний тип
                .Include(b => b.Copies)         // щоби знайти вільну копію для паперу
                .FirstOrDefaultAsync(b => b.Id == dto.BookId);

            if (bookEntity == null)
                throw new KeyNotFoundException($"Book with Id = {dto.BookId} not found.");

            // 2) Залежно від типу замовлення оновити поля книги та (опціонально) BookCopyId:
            switch (dto.OrderType)
            {
                case BookType.Paper:
                    {
                        // Знайти першу вільну копію (IsAvailable == true)
                        var freeCopy = bookEntity.Copies.FirstOrDefault(c => c.IsAvailable);
                        if (freeCopy == null)
                            throw new InvalidOperationException("No available paper copies to reserve.");

                        // Позначити цю копію як невільну
                        freeCopy.IsAvailable = false;

                        // Зменшити AvailableCopies
                        if (bookEntity.AvailableCopies < 1)
                            throw new InvalidOperationException("No paper copies available to reserve.");
                        bookEntity.AvailableCopies -= 1;
                        _uow.Books.Update(bookEntity);

                        // Указати BookCopyId у dto
                        dto.BookCopyId = freeCopy.Id;
                        break;
                    }
                case BookType.Audio:
                    {
                        // Якщо Audio, просто інкрементуємо ListenCount
                        bookEntity.ListenCount += 1;
                        dto.BookCopyId = null;
                        break;
                    }
                case BookType.Electronic:
                    {
                        // Якщо Electronic, інкрементуємо DownloadCount
                        bookEntity.DownloadCount += 1;
                        dto.BookCopyId = null;
                        break;
                    }
                default:
                    throw new InvalidOperationException("Unsupported BookType for order.");
            }

            var orderEntity = _mapper.Map<Order>(dto);

            // Ось тут явно задаємо дату
            orderEntity.OrderDate = DateTime.UtcNow;

            if (dto.OrderType == BookType.Paper)
            {
                orderEntity.Status = OrderStatus.Awaiting;
            }
            else
            {
                // для Audio і Electronic
                orderEntity.Status = OrderStatus.NoPaper;
            }

            // 5) Зберегти Order
            await _uow.Orders.CreateAsync(orderEntity);

            // 6) Оновити стан книги (щоби зберегти ListenCount/DownloadCount/AvailableCopies/IsAvailable у BookCopy)
            _uow.Books.Update(bookEntity);

            // 7) Якщо було Paper, також треба оновити BookCopy (щоби зберегти IsAvailable = false)
            if (dto.OrderType == BookType.Paper && dto.BookCopyId.HasValue)
            {
                var copyEntity = bookEntity.Copies.FirstOrDefault(c => c.Id == dto.BookCopyId.Value);
                if (copyEntity != null)
                    _uow.BookCopies.Update(copyEntity);
            }

            // 8) Комітимо усі зміни разом у одній транзакції
            await _uow.CommitAsync();
        }

        public async Task UpdateStatusAsync(int orderId)
        {
            var orderEntity = await _uow.Orders
                .ReadAllOrder()
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (orderEntity == null)
                throw new KeyNotFoundException($"Order with Id = {orderId} not found.");

            // Дозволяємо змінювати лише з “Awaiting” → “WithUser”
            if (orderEntity.Status != OrderStatus.Awaiting)
                throw new InvalidOperationException("Cannot change status: only orders in Awaiting can be moved to WithUser.");

            orderEntity.Status = OrderStatus.WithUser;
            _uow.Orders.Update(orderEntity);
            await _uow.CommitAsync();
        }


        public async Task DeleteAsync(int orderId, bool isAdmin)
        {
            // 1) Завантажити замовлення разом із Book і BookCopy (якщо було паперове)
            var orderEntity = await _uow.Orders
                .ReadAllOrder()                         // IQueryable<Order>
                .Include(o => o.Book)              // Щоби оновити Book.AvailableCopies
                .Include(o => o.BookCopy)          // Щоби звільнити копію (якщо це паперове замовлення)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (orderEntity == null)
                throw new KeyNotFoundException($"Order with Id = {orderId} not found.");
            if (orderEntity.Status == OrderStatus.WithUser && !isAdmin)
            {
                throw new UnauthorizedAccessException("Only admin can delete an order that is already with user.");
            }

            var bookEntity = orderEntity.Book;
            if (bookEntity == null)
                throw new InvalidOperationException("Associated book not found.");

            // 2) Якщо це паперова книга, звільнити копію й оновити AvailableCopies
            if (orderEntity.OrderTypeId == (int)BookType.Paper && orderEntity.BookCopyId.HasValue)
            {     
                var copyEntity = orderEntity.BookCopy;
                if (copyEntity == null)
                    throw new InvalidOperationException("Associated BookCopy not found.");

                // Звільняємо копію
                copyEntity.IsAvailable = true;
                _uow.BookCopies.Update(copyEntity);

                // Збільшуємо AvailableCopies
                bookEntity.AvailableCopies += 1;
                _uow.Books.Update(bookEntity);
            }
            // Для Audio/Electronic ми нічого не зменшуємо при видаленні замовлення—лічильники залишаються (бо це історія прослуховувань/завантажень)

            // 3) Видаляємо саме замовлення
            _uow.Orders.Delete(orderEntity);

            // 4) Комітимо всі зміни
            await _uow.CommitAsync();
        }
    }
}
