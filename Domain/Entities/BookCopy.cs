using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Domain.Entities
{
    public class BookCopy
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public Book Book { get; set; }

        public int CopyNumber { get; set; }

        public bool IsAvailable { get; set; }
    }
}
