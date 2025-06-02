using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class GenreViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
