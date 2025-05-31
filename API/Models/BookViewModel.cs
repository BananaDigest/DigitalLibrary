using Domain.Enums;

namespace API.Models
{
    public class BookViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public BookType Type { get; set; }          // із DigitalLibrary.Domain.Enums
        public int GenreId { get; set; }
    }
}
