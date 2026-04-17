namespace Viora.Models
{
    public class HelpComment
    {
        public int Id { get; set; }
        public int HelpPostId { get; set; }
        public int UserId { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public HelpPost HelpPost { get; set; }
        public ApplicationUser User { get; set; }
    }
}
