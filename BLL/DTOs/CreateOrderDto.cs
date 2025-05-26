using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class CreateOrderDto
    {
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public BookType OrderType { get; set; }
        public Guid? BookCopyId { get; set; }
    }
}
