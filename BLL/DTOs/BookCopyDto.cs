using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class BookCopyDto
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public int CopyNumber { get; set; }
        public bool IsAvailable { get; set; }
    }
}
