using Newtonsoft.Json;

namespace Viora.AIResponses
{
    public class ImageRawDataResult
    {
        [JsonProperty("<DETAILED_CAPTION>")]
        public string DetailedCaption { get; set; }
    }
}
