﻿using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class ActionOrderDto
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public BookType OrderType { get; set; }
        public int? BookCopyId { get; set; }
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
    }
}
