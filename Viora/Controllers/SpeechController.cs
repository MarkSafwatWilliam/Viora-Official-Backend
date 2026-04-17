using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System.IO.Pipelines;
using Viora.Services;
using Newtonsoft.Json;
using Viora.AIResponses;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;


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
        protected int CurrentUserId => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : 0;

        public SpeechController(SpeechToTextService speechToTextService, IntentClassificationService intentClassification, MessageHandlingService messageHandlingService)
        {
            _speechToTextService = speechToTextService;
            _intentClassification = intentClassification;
            _messageHandlingService = messageHandlingService;
        }

        [HttpPost("transcribe")]
        public async Task<IActionResult> Transcribe(IFormFile audioFile , int? chatId) {

            if (audioFile == null || audioFile.Length==0) {
                return BadRequest(new { error = "No audio file provided" });
            
            }
            try {

                var resultJsonString = await _speechToTextService.ConvertSpeechToTextAsync(audioFile.OpenReadStream(),audioFile.FileName);
                var result = JsonConvert.DeserializeObject<WhisperSpeechResult>(resultJsonString);

                if (result?.RecognitionStatus != "Success")
                    return StatusCode(500, new { error = "Transcription unsuccessful", details = result?.RecognitionStatus });

                string text = result.DisplayText;
                
                var chatID =await _messageHandlingService.SaveUserMessage(text, CurrentUserId ,chatId);


                var intentResult = await _intentClassification.ClassifyIntentAsync(text);
                if (!intentResult.IsSuccess)
                    return StatusCode(500, new { error = "Intent classification failed" });

                //switch (handle the different intents)
                return intentResult.Nlu.Intent switch
                {
                    "open_document" => await HandleOpenDocument(text, intentResult.Nlu.Entities , chatID),
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
    }
}
