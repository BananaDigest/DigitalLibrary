using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
