using Newtonsoft.Json;

namespace Viora.AIResponses
{
    public class SummarizeResult
    {
        [JsonProperty("status")]
        public string Status {  get; set; }

        [JsonProperty("summary")]
        public string Summary {  get; set; }
    }
}
