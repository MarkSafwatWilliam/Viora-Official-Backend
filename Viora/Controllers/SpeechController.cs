using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Pipelines;
using System.Reflection.Metadata;
using Viora.AIResponses;
using Viora.Dtos;
using Viora.Services;
using static System.Net.Mime.MediaTypeNames;


namespace Viora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SpeechController : ControllerBase
    {

        private readonly SpeechToTextService _speechToTextService;
        private readonly IntentClassificationService _intentClassification;
        private readonly MessageHandlingService _messageHandlingService;
        private readonly TextToSpeechService _ttsService;
        private readonly DocumentHandlingService _documentHandlingService;


        protected int CurrentUserId => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : 0;

        public SpeechController(SpeechToTextService speechToTextService, IntentClassificationService intentClassification,
            MessageHandlingService messageHandlingService, TextToSpeechService ttsService,
            DocumentHandlingService documentHandlingService)
        {
            _speechToTextService = speechToTextService;
            _intentClassification = intentClassification;
            _messageHandlingService = messageHandlingService;
            _ttsService = ttsService;
            _documentHandlingService = documentHandlingService;
        }

        [HttpPost("transcribe")]
        public async Task<IActionResult> Transcribe([FromForm] TranscribeRequestDTO request) {

            if (request.AudioFile == null || request.AudioFile.Length==0) {
                return BadRequest(new { error = "No audio file provided" });
            
            }
            try {

                var resultJsonString = await _speechToTextService.ConvertSpeechToTextAsync(request.AudioFile.OpenReadStream(),request.AudioFile.FileName);
                var result = JsonConvert.DeserializeObject<WhisperSpeechResult>(resultJsonString);

                if (result?.RecognitionStatus != "Success")
                    return StatusCode(500, new { error = "Transcription unsuccessful", details = result?.RecognitionStatus });

                string text = result.DisplayText;
                
                var chatID =await _messageHandlingService.SaveMessage(text, CurrentUserId , request.ChatId, "User");


                var intentResult = await _intentClassification.ClassifyIntentAsync(text);
                if (!intentResult.IsSuccess)
                    return StatusCode(500, new { error = "Intent classification failed" });

                //switch (handle the different intents)
                return intentResult.Nlu.Intent switch
                {
                    "open_document" => await HandleOpenDocument(text, intentResult.Nlu.Entities , chatID),
                    "summarize_content"=> await HandleContentSummarization(request.ChatId,request.DocumentId),
                    _ => BadRequest(new { error = "Unhandled intent", intent = intentResult.Nlu.Intent ,chatID })


                };

            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { error = "External service error", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
            }

        }




        private async Task<IActionResult> HandleOpenDocument(string text, IntentEntities entities , int chatId)
        {
            if (string.IsNullOrEmpty(entities.DocumentName))
                return BadRequest(new { error = "No document name found" });

            return Ok(new { STT_text = text, intent = "open_document", documentName = entities.DocumentName, chatId });
        }



        [HttpPost("summarize")]
        public async Task<IActionResult> HandleContentSummarization(int? chatId, int? documentId)
        {
            if (!documentId.HasValue)
                return BadRequest();

            var summary = await _documentHandlingService.SummarizePdf(documentId, CurrentUserId);

            await _messageHandlingService.SaveMessage(summary, CurrentUserId, chatId, "AI");

            return Ok(new { summary });
        }







        [HttpPost("tts")]
        [Produces("audio/mpeg")]

        public async Task<IActionResult> TextToSpeech([FromBody] string text)
        {
            var audioBytes = await _ttsService.ConvertTextToSpeech(text);

            return File(audioBytes, "audio/mpeg", "speech.mp3");
        }



    }
}
