namespace Viora.Models
{
    public class ChatSummary
    {
        public int Id { get; set; }

        public int ChatId { get; set; }
        public string SummaryText { get; set; }
        public int StartMessageId { get; set; } 
        public int EndMessageId { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Chat Chat { get; set; }
    }
}
