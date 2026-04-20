namespace Viora.Models
{
    public class UserFile
    {
        public int Id { get; set; }
        public int UserOwnerId { get; set; }

        public string? FileName { get; set; }
        public string? Path { get; set; }
        public long? Size { get; set; }
        public string? Type { get; set; }

        // AI processed content
        public string? Content { get; set; }
        public int? TextLength { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; }
    }
}
