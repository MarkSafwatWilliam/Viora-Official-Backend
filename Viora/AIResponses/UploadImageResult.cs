using Newtonsoft.Json;

namespace Viora.AIResponses
{
    public class UploadImageResult
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("result")]
        public string Description { get; set; }

        [JsonProperty("raw")]
        public ImageRawDataResult Raw { get; set; }
    }
}