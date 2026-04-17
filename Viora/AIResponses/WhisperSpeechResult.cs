using Newtonsoft.Json;

namespace Viora.AIResponses
{
    public class WhisperSpeechResult
    {
        [JsonProperty("text")]
        public string DisplayText { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        // Map "success" string to the "Success" check your controller uses
        public string RecognitionStatus =>
            Status?.ToLower() == "success" ? "Success" : "Failed";
    }
}
