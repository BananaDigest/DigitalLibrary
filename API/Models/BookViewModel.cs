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
        public string GenreName { get; set; }         // Підтягуємо через фасад
        public List<int> AvailableTypeIds { get; set; } = new();
        public int InitialCopies { get; set; }        // Нова властивість DTO → клієнту
        public int AvailableCopies { get; set; }      // Нова властивість DTO → клієнту
        public int DownloadCount { get; set; }        // Нова властивість DTO → клієнту
        public int ListenCount { get; set; }          // Нова властивість DTO → клієнту
        public string Description { get; set; }
    }
}
