using Domain.Enums;

namespace API.Models
{
    public class BookViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public int PublicationYear { get; set; }
        public int GenreId { get; set; }
        public string GenreName { get; set; }       
        public List<int> AvailableTypeIds { get; set; } = new();
        public int InitialCopies { get; set; }       
        public int AvailableCopies { get; set; }      
        public int DownloadCount { get; set; }        
        public int ListenCount { get; set; }      
        public string Description { get; set; }
    }
}
