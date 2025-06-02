using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class CreateOrderModel
    {
        [Required] public int UserId { get; set; }
        [Required] public int BookId { get; set; }
        [Required] public BookType OrderType { get; set; }
    }
}
