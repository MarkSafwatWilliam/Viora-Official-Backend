namespace Viora.AIResponses
{
    public class IntentResult
    {
        public string Status { get; set; }
        public string Decision { get; set; }
        public NluResult Nlu { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}
