using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Domain.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public int PublicationYear { get; set; }

        public int GenreId { get; set; }
        public Genre Genre { get; set; }
        public int InitialCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int DownloadCount { get; set; }
        public int ListenCount { get; set; }
        public string Description { get; set; }

        public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
        public ICollection<BookTypeEntity> AvailableTypes { get; set; } = new List<BookTypeEntity>();
    }
}
