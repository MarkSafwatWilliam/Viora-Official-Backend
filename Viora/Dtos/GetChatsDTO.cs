namespace Viora.Dtos
{
    public class GetChatsDTO
    {
        public int ChatId {  get; set; }
        public string? Title { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
