using BLL.DTOs;
using DAL.UnitOfWork;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Factory
{
    public class BookFactory : IBookFactory
    {
        private readonly IUnitOfWork _uow;

        public BookFactory(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }
        public async Task<Book> CreateAsync(ActionBookDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // 1) Ініціалізуємо основні поля Book
            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Publisher = dto.Publisher,
                PublicationYear = dto.PublicationYear,
                GenreId = dto.GenreId,
                AvailableTypes = new List<BookTypeEntity>()
            };

            // 2) Завантажуємо Genre з БД, щоби EF мав єдиний екземпляр
            var genreEntity = await _uow.Genres.ReadByIdAsync(dto.GenreId);
            if (genreEntity == null)
                throw new KeyNotFoundException($"Genre з Id = {dto.GenreId} не знайдено");

            book.Genre = genreEntity;

            // 3) Завантажуємо кожен BookTypeEntity із БД замість new BookTypeEntity { Id = ... }
            book.AvailableTypes = new List<BookTypeEntity>();
            foreach (var typeId in dto.AvailableTypeIds.Distinct())
            {
                var typeEntity = await _uow.BookTypes.ReadByIdAsync(typeId);
                if (typeEntity == null)
                    throw new KeyNotFoundException($"BookTypeEntity з Id = {typeId} не знайдено");

                book.AvailableTypes.Add(typeEntity);
            }

            // 4) Якщо серед AvailableTypeIds є паперовий ("Paper" має значення enum = 0),
            //    створюємо відповідні BookCopy для кожного екземпляра:
            if (dto.AvailableTypeIds.Contains((int)BookType.Paper))
            {
                book.Copies = new List<BookCopy>();
                for (int i = 1; i <= dto.CopyCount; i++)
                {
                    book.Copies.Add(new BookCopy
                    {
                        Book = book,
                        CopyNumber = i,
                        IsAvailable = true
                    });
                }
            }
            return book;
        }
    }
}
