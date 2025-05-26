using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid BookId { get; set; }
        public Book Book { get; set; }

        public BookType OrderType { get; set; }

        public Guid? BookCopyId { get; set; }
        public BookCopy BookCopy { get; set; }

        public DateTime OrderDate { get; set; }
    }
}
