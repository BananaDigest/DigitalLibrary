using Domain.Enums;

namespace BLL.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public int PublicationYear { get; set; }
        public List<int> AvailableTypeIds { get; set; } = new();
        public int GenreId { get; set; }
        public GenreDto Genre { get; set; }
        public string Description { get; set; }
        public int AvailableCopies { get; set; }  
        public int InitialCopies { get; set; }    
        public int DownloadCount { get; set; }    
        public int ListenCount { get; set; }      
    }
}
