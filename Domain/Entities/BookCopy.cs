using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Domain.Entities
{
    public class BookCopy
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }

        public int CopyNumber { get; set; }

        public bool IsAvailable { get; set; }
    }
}
