using System.Net.Http.Headers;
namespace Viora.Services
{
    public class SpeechToTextService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _speechToTextEndPoint;

        public SpeechToTextService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {

            _httpClientFactory = httpClientFactory;
            _speechToTextEndPoint = configuration["ApiSettings:SpeechToTextEndPoint"];
        }


        #region bad implementation loads entire file
        //public async Task<string> ConvertSpeechToTextAsync(byte[] audioData)
        //{

        //    var client = _httpClientFactory.CreateClient();

        //    //To create an HTTP content object from a byte array
        //    //ByteArrayContent : class used to send binary data in an HTTP request.

        //    var fileContent = new ByteArrayContent(audioData);

        //    //Sets the Content-Type header of the HTTP request.
        //    fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        //    var formData = new MultipartFormDataContent();
        //    formData.Add(fileContent, "file", "audio.wav");

        //    var response = await client.PostAsync(_speechToTextEndPoint, formData);
        //    var resultJson = await response.Content.ReadAsStringAsync();
        //    if (!response.IsSuccessStatusCode)
        //        throw new HttpRequestException($"Whisper S-T-T returned {response.StatusCode}: {resultJson}");


        //    return resultJson;

        //}
        #endregion

        //Better Implementation
        public async Task<string> ConvertSpeechToTextAsync(Stream audioStream, string fileName)
        {

            var client = _httpClientFactory.CreateClient();

            using var content = new MultipartFormDataContent();

            content.Add(new StreamContent(audioStream), "file", fileName);

            var response = await client.PostAsync(_speechToTextEndPoint, content);

            var resultJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Whisper returned {response.StatusCode}: {resultJson}");

            return resultJson;
        }
    }
}
