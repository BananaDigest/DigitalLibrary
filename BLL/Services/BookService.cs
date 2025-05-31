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
            var entities = await _uow.Books.ReadAllAsync();
            return _mapper.Map<IEnumerable<BookDto>>(entities);
        }

        public async Task<BookDto> ReadByIdAsync(int id)
        {
            var bookEntity = await _uow.Books
                .ReadAll()                           // IQueryable<Book>
                .Include(b => b.AvailableTypes)     // підвантажуємо join-колекцію
                .Include(b => b.Copies)             // підвантажуємо копії
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bookEntity == null)
                throw new KeyNotFoundException($"Book with Id = {id} not found.");
            var dto = _mapper.Map<BookDto>(bookEntity);
            return dto;
        }

        public async Task<IEnumerable<BookDto>> SearchAsync(string term)
        {
            var list = await _uow.Books.FindAsync(b =>
                b.Title.Contains(term) ||
                b.Author.Contains(term) ||
                b.Publisher.Contains(term));
            return _mapper.Map<IEnumerable<BookDto>>(list);
        }

        public async Task<IEnumerable<BookDto>> FilterByTypeAsync(int typeId)
        {
            // читаємо всі книги (або, краще — через репозиторій FindAsync, якщо реалізовано)
            var books = await _uow.Books.ReadAllAsync();
            // фільтруємо по зв’язаних сутностях BookType (ICollection<BookTypeEntity>)
            var filtered = books
                .Where(b => b.AvailableTypes.Any(bt => bt.Id == typeId))
                .ToList();
            return _mapper.Map<IEnumerable<BookDto>>(filtered);
        }

        public async Task<IEnumerable<BookDto>> FilterByGenreAsync(int genreId)
        {
            var list = await _uow.Books.FindAsync(b => b.GenreId == genreId);
            return _mapper.Map<IEnumerable<BookDto>>(list);
        }

        public async Task CreateAsync(ActionBookDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // 1) Викликаємо фабрику, яка повертає готовий Book з усіма зв’язками:
            var bookEntity = await _factory.CreateAsync(dto);

            // 2) Додаємо новий Book у БД:
            await _uow.Books.CreateAsync(bookEntity);

            // 3) Комітимо зміни:
            await _uow.CommitAsync();
        }

        public async Task UpdateAsync(int bookId, ActionBookDto dto)
        {
            // 1) Завантажуємо книгу разом із навігаційними властивостями
            var bookEntity = await _uow.Books.ReadByIdAsync(bookId);

            if (bookEntity == null)
                throw new KeyNotFoundException($"Book with Id = {bookId} not found.");

            // 2) Оновлюємо прості поля
            bookEntity.Title = dto.Title;
            bookEntity.Author = dto.Author;
            bookEntity.Publisher = dto.Publisher;
            bookEntity.PublicationYear = dto.PublicationYear;
            bookEntity.GenreId = dto.GenreId;

            // 3) Оновлюємо AvailableTypes (join-таблицю book ↔ booktype)
            //    a) Спершу видалимо ті типи, яких більше немає у новому списку
            var toRemove = bookEntity.AvailableTypes
                .Where(bt => !dto.AvailableTypeIds.Contains(bt.Id))
                .ToList();
            foreach (var r in toRemove)
                bookEntity.AvailableTypes.Remove(r);

            //    b) Потім додамо ті типи, яких ще немає у колекції
            var existingTypeIds = bookEntity.AvailableTypes.Select(bt => bt.Id).ToList();
            var toAddIds = dto.AvailableTypeIds
                .Where(id => !existingTypeIds.Contains(id))
                .ToList();

            foreach (var typeId in toAddIds)
            {
                var typeEntity = await _uow.BookTypes.ReadByIdAsync(typeId);
                if (typeEntity == null)
                    throw new KeyNotFoundException($"BookTypeEntity with Id = {typeId} not found.");
                bookEntity.AvailableTypes.Add(typeEntity);
            }

            // 4) Оновлюємо Copies (якщо вводили CopyCount)
            //    Якщо у dto.AvailableTypeIds є Paper (0), потрібно оновити кількість записів у Copies:
            bookEntity.Copies.Clear();
            if (dto.AvailableTypeIds.Contains((int)BookType.Paper))
            {
                for (int i = 1; i <= dto.CopyCount; i++)
                {
                    bookEntity.Copies.Add(new BookCopy
                    {
                        CopyNumber = i,
                        BookId = bookEntity.Id,
                        IsAvailable = true
                    });
                }
            }

            // 5) Комітимо зміни — EF Core подбає про join-таблицю автоматично
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
