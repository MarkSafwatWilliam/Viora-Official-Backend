namespace Viora.Dtos
{
    public class TranscribeJsonResponseDTO
    {
        public string SttText { get; set; }
        public string Intent { get; set; }
        public string DocumentName { get; set; }
        public int ChatId { get; set; }
    }
}
