using Domain.Enums;

namespace API.Models
{
    public class BookViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public BookType Type { get; set; }          // із DigitalLibrary.Domain.Enums
        public Guid GenreId { get; set; }
    }
}
