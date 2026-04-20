using Azure;

namespace Viora.Services
{
    public class TextToSpeechService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _textToSpeechEndPoint;

        public TextToSpeechService(IHttpClientFactory httpClientFactory,IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _textToSpeechEndPoint = configuration["ApiSettings:TextToSpeechEndPoint"];
        }

        public async Task<byte[]> ConvertTextToSpeech(string userText) { 
        
            var client = _httpClientFactory.CreateClient();
            var requestBody = new
            {
                text = userText,
                temperature = 0.7,
                exaggeration = 0.7
            };
            var repsonse = await client.PostAsJsonAsync(_textToSpeechEndPoint, requestBody);

            if (!repsonse.IsSuccessStatusCode)
                throw new HttpRequestException("Text to Speech Model returned:" +repsonse.StatusCode);

            return await repsonse.Content.ReadAsByteArrayAsync();
        }
    }
}
