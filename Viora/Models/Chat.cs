namespace Viora.Models
{
    public class Chat
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string Title { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastAccessedAt { get; set; }

        public bool IsPinned { get; set; } = false;
        public bool IsArchived { get; set; } = false;

        //To access without querying summaries
        public string? LastSummary { get; set; }

        //Navigation properties
        public ApplicationUser User { get; set; }

        public ICollection<ChatMessage> Messages { get; set; }
        public ICollection<ChatSummary> Summaries { get; set; }


    }
}
