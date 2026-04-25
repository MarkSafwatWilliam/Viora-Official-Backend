using Azure;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;
using Viora.AIResponses;
using Microsoft.Extensions.Configuration;

namespace Viora.Services
{
    public class IntentClassificationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _intentEndPoint;

        public IntentClassificationService(IHttpClientFactory httpClientFactory, IConfiguration configuration) {
        
            _httpClientFactory = httpClientFactory;
            _intentEndPoint = configuration["ApiSettings:IntentEndPoint"];

        }

        public async Task<IntentResult> ClassifyIntentAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new IntentResult { IsSuccess = false, ErrorMessage = "Text is empty" };

            try
            {
                var client = _httpClientFactory.CreateClient();
                var payload = new StringContent(
                    JsonConvert.SerializeObject(new { text = text }),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(_intentEndPoint, payload);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new IntentResult { IsSuccess = false, ErrorMessage = $"Service returned {(int)response.StatusCode}: {error}" };
                }

                var rawResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IntentResult>(rawResult);
                result.IsSuccess = true;

                return result;
            }
            catch (Exception ex)
            {
                return new IntentResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }




    }
}
