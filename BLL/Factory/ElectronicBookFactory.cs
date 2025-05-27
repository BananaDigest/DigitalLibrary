using BLL.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Factory
{
    public class ElectronicBookFactory : BookFactory
    {
        public override Book Create(ActionBookDto dto)
        {
            return new Book
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                Title = dto.Title,
                Author = dto.Author,
                Publisher = dto.Publisher,
                PublicationYear = dto.PublicationYear,
                AvailableTypes = dto.AvailableTypes,
                GenreId = dto.GenreId,
                Copies = new List<BookCopy>() // no physical copies
            };
        }
    }
}
