using Azure;
using Azure.Core;
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


        /// <summary>
        /// Converts an audio file to text and saves it as a user message.
        /// </summary>
        /// <remarks>
        /// Workflow: audio → transcript → save as User message → return result
        ///
        /// **Request (multipart/form-data):**
        /// - AudioFile : Required. WAV or MP3 file.
        /// - ChatId    : Optional. Existing chat ID; a new chat is created if omitted.
        ///
        /// **Response (application/json):**
        /// - 200 → { text, chatId }
        /// - 400 → No audio file provided.
        /// - 500 → Transcription service failed.
        /// - 502 → External service error.
        /// 
        /// Requires a valid JWT bearer token.
        /// </remarks>
        /// <response code="400">No audio file was provided, or the file is empty.</response>
        /// <response code="500">The transcription service returned a non-success status.</response>
        /// <response code="502">An error occurred while communicating with the external speech-to-text service.</response>
        [HttpPost("stt")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]                                          
        [ProducesResponseType(typeof(SttResponseDTO), StatusCodes.Status200OK)]  
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SpeechToText([FromForm] TranscribeRequestDTO request)
        {

            if (request.AudioFile == null || request.AudioFile.Length == 0)
                return BadRequest(new { error = "No audio file provided" });

            try
            {
                var result = await _speechToTextService.ConvertSpeechToTextAsync(request.AudioFile.OpenReadStream(), request.AudioFile.FileName);
                if (result?.RecognitionStatus != "Success")
                    return StatusCode(500, new { error = "Transcription unsuccessful", details = result?.RecognitionStatus });

                string text = result.DisplayText;

                var chatID = await _messageHandlingService.SaveMessage(text, CurrentUserId, request.ChatId, "User");


                return Ok(new { text = text, chatId = chatID });

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



        /// <summary>
        /// Converts speech to text, classifies intent, and executes the corresponding action.
        /// </summary>
        /// <remarks>
        /// Workflow: audio → transcript → intent detection → action
        ///
        /// **Request (multipart/form-data):**
        /// - AudioFile  : Required. WAV or MP3 file.
        /// - ChatId     : Optional. Existing chat ID; a new chat is created if omitted.
        /// - DocumentId : Required for summarize_content, document_qa, and generate_study_aid.
        ///
        /// **Intent → Response:**
        /// - open_document      → JSON: { sttText, intent, documentName, chatId }
        /// - summarize_content  → MP3 audio
        /// - document_qa        → MP3 audio
        /// - generate_study_aid → MP3 audio
        /// - unknown intent     → 400 JSON: { error, intent, chatId }
        ///
        /// **Frontend must check Content-Type:**
        /// - application/json → parse as JSON
        /// - audio/mpeg       → play audio
        /// 
        /// Requires a valid JWT bearer token.
        /// </remarks>
        ///         
        /// <response code="500">Transcription failed or intent classification returned an error.</response>
        /// <response code="502">An error occurred while communicating with an external service.</response>
        [HttpPost("transcribe")]
        [Consumes("multipart/form-data")]
        [Produces("application/json", "audio/mpeg")]
        [ProducesResponseType(typeof(TranscribeJsonResponseDTO), StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "audio/mpeg")]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Transcribe([FromForm] TranscribeRequestDTO request) {

            if (request.AudioFile == null || request.AudioFile.Length==0) {
                return BadRequest(new { error = "No audio file provided" });
            
            }
            try {

                var result = await _speechToTextService.ConvertSpeechToTextAsync(request.AudioFile.OpenReadStream(),request.AudioFile.FileName);

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
                    "document_qa"=> await HandleQuestionsAndAnswers(request.ChatId,request.DocumentId,intentResult.Nlu.Entities.Question ),
                    "generate_study_aid"=> await HandleMaterialGeneration(request.ChatId,request.DocumentId),
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


        private async Task<IActionResult> HandleContentSummarization(int? chatId, int? documentId)
        {
            if (!documentId.HasValue)
                return BadRequest();

            var summary = await _documentHandlingService.SummarizePdf(documentId, CurrentUserId);

            await _messageHandlingService.SaveMessage(summary, CurrentUserId, chatId, "AI");
            var audio = await _ttsService.ConvertTextToSpeech(summary);


            return File(audio, "audio/mpeg", "summary.mp3");
        }



        private async Task<IActionResult> HandleQuestionsAndAnswers(int? chatId, int? documentId,string question) {

            if (!documentId.HasValue || String.IsNullOrEmpty(question))
                return BadRequest();

            var response = await _documentHandlingService.QuestionsAnswers(documentId, CurrentUserId,question);

            await _messageHandlingService.SaveMessage(response, CurrentUserId, chatId, "AI");

            var audio = await _ttsService.ConvertTextToSpeech(response);

            return File(audio, "audio/mpeg", "response.mp3");

        }


        private async Task<IActionResult> HandleMaterialGeneration(int? chatId , int? documentId)
        {
            var quiz= await _documentHandlingService.GeneratingStudyMaterials(documentId, CurrentUserId);
            if (!documentId.HasValue || String.IsNullOrEmpty(quiz))
                return BadRequest();

            await _messageHandlingService.SaveMessage(quiz, CurrentUserId, chatId, "AI");

            var audio = await _ttsService.ConvertTextToSpeech(quiz);

            return File(audio, "audio/mpeg", "quiz.mp3");
        }


        /// <summary>
        /// Converts a text string to speech and returns it as an MP3 audio file.
        /// </summary>
        /// <remarks>
        /// Accepts a JSON string body and returns the synthesized audio.
        ///
        /// Requires a valid JWT bearer token.
        /// </remarks>
        /// <param name="text">
        /// Required. A JSON-encoded string containing the text to synthesize.
        /// Example body: <c>"Hello, this is Viora."</c>
        /// </param>
        /// <returns>An MP3 audio file of the synthesized speech.</returns>
        /// <response code="200">Returns the synthesized audio as an MP3 file (Content-Type: audio/mpeg).</response>
        [HttpPost("tts")]
        [Produces("audio/mpeg")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> TextToSpeech([FromBody] string text)
        {
            var audioBytes = await _ttsService.ConvertTextToSpeech(text);

            return File(audioBytes, "audio/mpeg", "speech.mp3");
        }



    }
}
