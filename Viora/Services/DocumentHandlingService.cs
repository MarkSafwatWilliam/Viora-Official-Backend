using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Viora.AIResponses;
using Viora.Models;
using Viora.Repositories;

namespace Viora.Services
{
    public class DocumentHandlingService
    {
        private readonly string _uploadDocumentEndPoint;
        private readonly string _summarizationEndPoint;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly UserFileRepository _repo;

        public DocumentHandlingService(IConfiguration configuration, IHttpClientFactory httpClientFactory, IMemoryCache cache,
            UserFileRepository repo)
        {

            _uploadDocumentEndPoint = configuration["ApiSettings:UploadDocumentEndPoint"];
            _summarizationEndPoint = configuration["ApiSettings:SummarizationEndPoint"];
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _repo = repo;

        }


        public async Task<int> UploadPdf(IFormFile pdf,string fileName,int userId) { 
        
            var client = _httpClientFactory.CreateClient();

            using var content = new MultipartFormDataContent();
            using var stream = pdf.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(pdf.ContentType);
            content.Add(fileContent, "file", fileName);

            var response =await client.PostAsync(_uploadDocumentEndPoint, content);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"AI Service returned {response.StatusCode}");

            var jsonString = await response.Content.ReadAsStringAsync();
            var uploadResponse = JsonConvert.DeserializeObject<UploadResult>(jsonString);


            if (uploadResponse == null || uploadResponse.PdfContent == null)
                throw new InvalidOperationException("AI Service returned empty or invalid response.");

            if (uploadResponse.TextLength > 200_000)
            {
                throw new InvalidOperationException("File too large for processing.");
            }

            var file = new UserFile()
            {
                UserOwnerId = userId,
                Content = uploadResponse.PdfContent,
                FileName = fileName,
                TextLength = uploadResponse.TextLength,
                Size = pdf.Length,
                Type = pdf.ContentType
            };
            //Save to DB
             await _repo.Add(file);

            await _repo.SaveChanges();


            //Caching small docs
            if (uploadResponse.TextLength < 50_000)
            {

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(4));

                _cache.Set($"pdf:{file.Id}", uploadResponse.PdfContent, cacheOptions);
            }

            return file.Id;
        }


        public async Task<string> SummarizePdf(int? documentId,int userId)
        {
            var client = _httpClientFactory.CreateClient();

            var content = await GetPdfContentAsync(documentId, userId);

            var summary = await GetSummaryFromAiAsync(client, content);

            return summary;
        }

        private async Task<string> GetPdfContentAsync(int? documentId,int userId)
        {
            if (_cache.TryGetValue($"pdf:{documentId}", out string cachedContent))
            {
                return cachedContent;
            }

            var userFile = await _repo.GetById(documentId, userId);

            if (userFile == null || string.IsNullOrEmpty(userFile.Content))
                throw new Exception("File content not found");

            return userFile.Content;
        }


        private async Task<string> GetSummaryFromAiAsync(HttpClient client, string content)
        {
            var response = await client.PostAsJsonAsync(
                _summarizationEndPoint,
                new { content, mode = "summary" }
            );

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"AI Service returned {response.StatusCode}");

            var stringJson = await response.Content.ReadAsStringAsync();

            var summaryResult = JsonConvert.DeserializeObject<SummarizeResult>(stringJson);

            if (summaryResult?.Status != "success")
                throw new HttpRequestException("Summarization failed");

            return summaryResult.Summary;
        }

    }
}
