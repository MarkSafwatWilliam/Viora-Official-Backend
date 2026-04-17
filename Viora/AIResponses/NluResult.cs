namespace Viora.AIResponses
{
    public class NluResult
    {
        public string TranslatedText { get; set; }
        public string Intent { get; set; }
        public float Confidence { get; set; }
        public IntentEntities Entities { get; set; }

        public bool NeedsClarification { get; set; }
    }
}
