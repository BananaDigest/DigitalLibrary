using BLL.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Factory
{
    public class AudioBookFactory : BookFactory
    {
        public override Book Create(ActionBookDto dto)
        {
            return new Book
            {
                Id = dto.Id == 0 ? 0 : dto.Id,
                Title = dto.Title,
                Author = dto.Author,
                Publisher = dto.Publisher,
                PublicationYear = dto.PublicationYear,
                AvailableTypes = dto.AvailableTypeIds
                               .Select(id => new BookTypeEntity { Id = id })
                               .ToList(),
                GenreId = dto.GenreId,
                Copies = new List<BookCopy>() // аудіо копій не буває
            };
        }
    }
}
