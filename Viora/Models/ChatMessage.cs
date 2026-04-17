namespace Viora.Models
{
    public class ChatMessage
    {

        public int Id { get; set; }

        public int ChatId { get; set; }

        public int UserId { get; set; }
        public string Content { get; set; }

        public string SenderType { get; set; } //User or AI

        public string ? AudioUrl {  get; set; }

        public int? TokenCount { get; set; }

        public bool IsSummarized { get; set; } = false;
        public bool IsDeleted { get; set; } = false;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        //Navigation properties
        public Chat Chat { get; set; }
        public ApplicationUser User { get; set; }
    }
}
