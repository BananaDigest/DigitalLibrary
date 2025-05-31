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
                Id = dto.Id == 0 ? 0 : dto.Id,
                Title = dto.Title,
                Author = dto.Author,
                Publisher = dto.Publisher,
                PublicationYear = dto.PublicationYear,
                AvailableTypes = dto.AvailableTypeIds
                               .Select(id => new BookTypeEntity { Id = id })
                               .ToList(),
                GenreId = dto.GenreId
            };
                book.Copies = new List<BookCopy>();
                for (int i = 1; i <= 5; i++) // дефолтно 5 копій
                {
                    book.Copies.Add(new BookCopy
                    {
                        Book = book,
                        CopyNumber = i,
                        IsAvailable = true
                    });
                }
            return book;
        }
    }

}
