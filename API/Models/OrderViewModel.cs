namespace API.Models
{
    public class OrderViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BookCopyId { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
