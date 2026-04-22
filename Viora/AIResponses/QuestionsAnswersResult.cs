using Newtonsoft.Json;

namespace Viora.AIResponses
{
    public class QuestionsAnswersResult
    {
        [JsonProperty("status")]
        public string Status;

        [JsonProperty("response")]
        public string Response;

    }
}
