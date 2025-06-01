using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class ActionBookDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public int PublicationYear { get; set; }
        public int GenreId { get; set; }
        public int Id { get; set; }
        public List<int> AvailableTypeIds { get; set; } = new();
        public int CopyCount { get; set; } = 0;
        public string Description { get; set; }
    }
}
