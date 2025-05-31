using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BLL.DTOs;
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

        public BookService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BookDto>> ReadAllAsync()
        {
            var entities = await _uow.Books.ReadAllAsync();
            return _mapper.Map<IEnumerable<BookDto>>(entities);
        }

        public async Task<BookDto> ReadByIdAsync(int id)
        {
            var book = await _uow.Books.ReadByIdAsync(id)
                ?? throw new KeyNotFoundException($"Book {id} not found");
            return _mapper.Map<BookDto>(book);
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
            var entity = _mapper.Map<Book>(dto);
            await _uow.Books.CreateAsync(entity);
            await _uow.CommitAsync();
        }

        public async Task UpdateAsync(ActionBookDto dto)
        {
            var existing = await _uow.Books.ReadByIdAsync(dto.Id)
                ?? throw new KeyNotFoundException($"Book {dto.Id} not found");
            _mapper.Map(dto, existing);
            _uow.Books.Update(existing);
            await _uow.CommitAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _uow.Books.ReadByIdAsync(id)
                ?? throw new KeyNotFoundException($"Book {id} not found");
            _uow.Books.Delete(existing);
            await _uow.CommitAsync();
        }
    }
}
