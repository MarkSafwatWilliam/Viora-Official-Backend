namespace Viora.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }

        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime ExpiredAt { get; set; }

        public bool IsRevoked { get; set; }
        
        public DateTime RevokedAt { get; set; }


        //Navigation property
        public ApplicationUser User { get; set; }

    }
}
