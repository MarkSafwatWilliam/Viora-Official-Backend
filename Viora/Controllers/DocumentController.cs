using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using Viora.Services;

namespace Viora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class DocumentController : ControllerBase
    {
        private readonly DocumentHandlingService _documentService;
        private readonly TextToSpeechService _textToSpeechService;

        protected int CurrentUserId => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : 0;

        public DocumentController(DocumentHandlingService documentService, TextToSpeechService textToSpeechService) { 
        
            _documentService = documentService;
            _textToSpeechService = textToSpeechService;
        }
        [HttpPost("upload-file")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [SwaggerResponse(200,"Successfully uploaded and you get the documentId")]
        [SwaggerResponse(400,"The file is empty or not in the correct format pdf")]
        public async Task<IActionResult> UploadFile(IFormFile file) 
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (file.ContentType != "application/pdf")
            {
                return BadRequest("The file provided is not a PDF.");
            }

            string fileName = file.FileName;

              var documentId= await _documentService.UploadPdf(file, fileName, CurrentUserId);

            return Ok(new { documentId });
        }



        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No image uploaded.");

            var description = await _documentService.UploadImage(file);
            if (string.IsNullOrEmpty(description))
            {
                return BadRequest("Failed to process the image.");
            }

            var audioStream = await _textToSpeechService.ConvertTextToSpeech(description);



            return File(audioStream, "audio/mpeg", "description.mp3");
        }
    }
}
