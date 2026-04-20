using System.ComponentModel.DataAnnotations;

namespace Viora.Dtos
{
    public class TranscribeRequestDTO
    {
        [Required]
        public IFormFile AudioFile { get; set; }
        public int? ChatId { get; set; }
        public int? DocumentId { get; set; }
    }
}
