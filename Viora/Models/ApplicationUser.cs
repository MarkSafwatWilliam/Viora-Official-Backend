using Microsoft.AspNetCore.Identity;

namespace Viora.Models
{
    public class ApplicationUser:IdentityUser<int>
    {
        public string DisplayName { get; set; }
        public string MotherName  { get; set; }
        public string CityOfBirth { get; set; }

        public bool IsHelper { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Navigation properties
        public ICollection<Chat> Chats { get; set; }
        public ICollection<ChatMessage> Messages { get; set; }

        public ICollection<UserFile> Files { get; set; }

        public ICollection<HelpPost> HelpPosts { get; set; }

        public ICollection<HelpComment> HelpComments { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }

    }
}
