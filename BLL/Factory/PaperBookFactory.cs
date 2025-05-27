using BLL.DTOs;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Factory
{
    public class PaperBookFactory : BookFactory
    {
        public override Book Create(ActionBookDto dto)
        {
            var book = new Book
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                Title = dto.Title,
                Author = dto.Author,
                Publisher = dto.Publisher,
                PublicationYear = dto.PublicationYear,
                AvailableTypes = dto.AvailableTypes,
                GenreId = dto.GenreId
            };
            // Initialize copies if Paper
            if (dto.AvailableTypes.HasFlag(BookType.Paper))
            {
                book.Copies = new List<BookCopy>();
                for (int i = 1; i <= 5; i++) // default 5 copies
                {
                    book.Copies.Add(new BookCopy
                    {
                        Id = Guid.NewGuid(),
                        BookId = book.Id,
                        CopyNumber = i,
                        IsAvailable = true
                    });
                }
            }
            return book;
        }
    }

}
