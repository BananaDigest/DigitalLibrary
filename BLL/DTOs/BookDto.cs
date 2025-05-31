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

        // Додайте ці нові властивості
        public int AvailableCopies { get; set; }  // Доступні паперові копії
        public int InitialCopies { get; set; }    // Початкова кількість паперових копій
        public int DownloadCount { get; set; }    // Кількість завантажень (електронна версія)
        public int ListenCount { get; set; }      // Кількість прослуховувань (аудіо версія)
    }
}
