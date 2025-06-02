using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class CreateBookModel
    {
        [Required] public string Title { get; set; }
        [Required] public string Author { get; set; }
        public string Publisher { get; set; }
        [Range(0, 3000)] public int PublicationYear { get; set; }
        [Required] public int GenreId { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "Потрібно вказати хоча б один тип книги")]
        public List<int> AvailableTypeIds { get; set; } = new();
        [Range(0, int.MaxValue)] public int CopyCount { get; set; } = 0;
        public string Description { get; set; }
    }
}
