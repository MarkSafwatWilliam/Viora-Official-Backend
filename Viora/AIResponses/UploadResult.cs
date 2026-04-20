using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Viora.AIResponses
{
    public class UploadResult
    {
        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("text_length")]
        public int? TextLength { get; set; }

        [JsonProperty("chunks")]
        public int? Chunks { get; set; }

        [JsonProperty("text")]
        public string? PdfContent { get; set; }
    }
}
