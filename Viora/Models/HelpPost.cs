namespace Viora.Models
{
    public class HelpPost
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // Open, Closed, Resolved

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ApplicationUser User { get; set; }
        public ICollection<HelpComment> Comments { get; set; }
    }
}
