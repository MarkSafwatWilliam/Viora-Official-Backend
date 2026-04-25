using Newtonsoft.Json;

namespace Viora.AIResponses
{
    public class IntentEntities
    {
        [JsonProperty("search_query")]
        public string SearchQuery { get; set; }

        [JsonProperty("file_types")]
        public List<string> FileTypes { get; set; }  

        [JsonProperty("document_name")]
        public string DocumentName { get; set; }

        [JsonProperty("page_number")]
        public string PageNumber { get; set; }

        [JsonProperty("navigation_direction")]
        public string NavigationDirection { get; set; }

        [JsonProperty("reading_action")]
        public string ReadingAction { get; set; }

        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("study_aid_type")]
        public string StudyAidType { get; set; }

        [JsonProperty("summary_format")]
        public string SummaryFormat { get; set; }

        [JsonProperty("focus_status")]
        public string FocusStatus { get; set; }

        [JsonProperty("camera_action")]
        public string CameraAction { get; set; }
    }
}