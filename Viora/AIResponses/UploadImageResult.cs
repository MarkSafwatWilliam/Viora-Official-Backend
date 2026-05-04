using Newtonsoft.Json;

namespace Viora.AIResponses
{
    public class UploadImageResult
    {

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("viora_reply")]
        public string Description { get; set; }
    }
}
