using System.ComponentModel.DataAnnotations;

namespace Viora.Dtos
{
    public class TranscribeRequestDTO
    {
        /// <summary>
        /// Audio file (wav or mp3)
        /// </summary>
        [Required]
        public IFormFile AudioFile { get; set; }

        /// <summary>
        /// Optional chat ID for conversation tracking
        /// </summary>
        public int? ChatId { get; set; }

        /// <summary>
        /// Optional document ID (required for summarization, Q&A, etc.)
        /// </summary>
        public int? DocumentId { get; set; }
    }
}
