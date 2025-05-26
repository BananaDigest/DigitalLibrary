using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class UpdateOrderDto
    {
        public BookType OrderType { get; set; }
        public Guid? BookCopyId { get; set; }
        public Guid Id { get; set; }
    }
}
