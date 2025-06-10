using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using BLL.DTOs;
using BLL.Factory;
using BLL.Interfaces;
using DAL.UnitOfWork;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace BLL.Services
{

    public class BookService : IBookService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IBookFactory _factory;

        public BookService(IUnitOfWork uow, IMapper mapper, IBookFactory factory)
        {
            _uow = uow;
            _mapper = mapper;
            _factory = factory;
        }

        public async Task<IEnumerable<BookDto>> ReadAllAsync()
        {
            var books = await _uow.Books
                .ReadAll()
                .Include(b => b.AvailableTypes)
                .Include(b => b.Copies)
                .Include(b => b.Genre)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BookDto>>(books);
        }

        public async Task<BookDto> ReadByIdAsync(int id)
        {
            var bookEntity = await _uow.Books
                .ReadAll()                         
                .Include(b => b.AvailableTypes)   
                .Include(b => b.Copies)             
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bookEntity == null)
                throw new KeyNotFoundException($"Book with Id = {id} not found.");
            var dto = _mapper.Map<BookDto>(bookEntity);
            return dto;
        }

        public async Task<List<BookDto>> ReadByTypeAsync(int typeId)
        {
            var entities = await _uow.Books
                .ReadAll()
                .Include(b => b.AvailableTypes)
                .Include(b => b.Copies)
                .Where(b => b.AvailableTypes.Any(t => t.Id == typeId))
                .ToListAsync();

            return _mapper.Map<List<BookDto>>(entities);
        }

        public async Task CreateAsync(ActionBookDto dto)
        {
            var bookEntity = _mapper.Map<Book>(dto);

            // Створюємо навігаційну колекцію AvailableTypes
            bookEntity.AvailableTypes = new List<BookTypeEntity>();
            foreach (var typeId in dto.AvailableTypeIds.Distinct())
            {
                var typeEntity = await _uow.BookTypes.ReadByIdAsync(typeId)
                                 ?? throw new KeyNotFoundException($"BookTypeEntity with Id = {typeId} not found.");
                bookEntity.AvailableTypes.Add(typeEntity);
            }

            // Створюємо копії лише якщо є паперовий тип
            bookEntity.Copies = new List<BookCopy>();
            if (dto.AvailableTypeIds.Contains((int)BookType.Paper))
            {
                // кількість copies = dto.CopyCount
                for (int i = 1; i <= dto.CopyCount; i++)
                {
                    bookEntity.Copies.Add(new BookCopy
                    {
                        CopyNumber = i,
                        IsAvailable = true
                    });
                }
            }

            await _uow.Books.CreateAsync(bookEntity);
            await _uow.CommitAsync();
        }

        public async Task UpdateAsync(int bookId, ActionBookDto dto)
        {
            var bookEntity = await _uow.Books
        .ReadAll() 
        .Include(b => b.AvailableTypes)
        .Include(b => b.Copies)
        .FirstOrDefaultAsync(b => b.Id == bookId);

            if (bookEntity == null)
                throw new KeyNotFoundException($"Book with Id = {bookId} not found.");

            // Оновлюємо основні поля
            bookEntity.Title = dto.Title;
            bookEntity.Author = dto.Author;
            bookEntity.Publisher = dto.Publisher;
            bookEntity.PublicationYear = dto.PublicationYear;
            bookEntity.GenreId = dto.GenreId;

            bookEntity.InitialCopies = dto.CopyCount;

            bookEntity.AvailableCopies = dto.CopyCount;
            bookEntity.Description = dto.Description;

            // Оновлюємо AvailableTypes (як раніше)
            var toRemove = bookEntity.AvailableTypes
                .Where(bt => !dto.AvailableTypeIds.Contains(bt.Id))
                .ToList();
            foreach (var oldType in toRemove)
                bookEntity.AvailableTypes.Remove(oldType);

            var existingTypeIds = bookEntity.AvailableTypes.Select(bt => bt.Id).ToList();
            var toAddIds = dto.AvailableTypeIds
                .Where(id => !existingTypeIds.Contains(id))
                .Distinct()
                .ToList();
            foreach (var typeId in toAddIds)
            {
                var typeEntity = await _uow.BookTypes.ReadByIdAsync(typeId)
                                 ?? throw new KeyNotFoundException($"BookTypeEntity with Id = {typeId} not found.");
                bookEntity.AvailableTypes.Add(typeEntity);
            }

            // Оновлюємо Copies (повністю очищаємо старі -> додаємо нові)
            bookEntity.Copies.Clear();
            if (dto.AvailableTypeIds.Contains((int)BookType.Paper))
            {
                for (int i = 1; i <= dto.CopyCount; i++)
                {
                    bookEntity.Copies.Add(new BookCopy
                    {
                        BookId = bookEntity.Id,
                        CopyNumber = i,
                        IsAvailable = true
                    });
                }
            }

            await _uow.CommitAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var bookEntity = await _uow.Books.ReadByIdAsync(id);
            if (bookEntity == null)
                throw new KeyNotFoundException($"Book with Id = {id} not found.");
            _uow.Books.Delete(bookEntity);
            await _uow.CommitAsync();
        }
    }
}
