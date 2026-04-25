// NluResult.cs
using Newtonsoft.Json;

namespace Viora.AIResponses
{
    public class NluResult
    {
        [JsonProperty("viora_reply")]
        public string VioraReply { get; set; }

        [JsonProperty("intent")]
        public string Intent { get; set; }

        [JsonProperty("confidence")]
        public float Confidence { get; set; }

        [JsonProperty("entities")]
        public IntentEntities Entities { get; set; }

        [JsonProperty("needs_clarification")]
        public bool NeedsClarification { get; set; }
    }
}