// IntentResult.cs
using Newtonsoft.Json;

namespace Viora.AIResponses
{
    public class IntentResult
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("decision")]
        public string Decision { get; set; }

        [JsonProperty("nlu")]
        public NluResult Nlu { get; set; }

        [JsonIgnore] 
        public bool IsSuccess { get; set; }

        [JsonIgnore]  
        public string ErrorMessage { get; set; }
    }
}