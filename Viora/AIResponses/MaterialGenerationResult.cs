using Newtonsoft.Json;

namespace Viora.AIResponses
{
    public class MaterialGenerationResult
    {
        [JsonProperty("status")]
        public string Status { get; set; }


        [JsonProperty("result")]
        public string Result { get; set; }
    }
}
