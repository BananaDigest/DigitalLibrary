using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; }
        public int OrderTypeId { get; set; }

        public BookTypeEntity OrderType { get; set; }

        public int? BookCopyId { get; set; }
        public BookCopy BookCopy { get; set; }

        public DateTime OrderDate { get; set; }
    }
}
