using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class UserActionModel
    {
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }

        [Required]
        [RegularExpression(
        @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
        ErrorMessage = "Невірний формат електронної адреси"
        )]
        public string Email { get; set; }

        [Required] public string Password { get; set; }
    }
}
