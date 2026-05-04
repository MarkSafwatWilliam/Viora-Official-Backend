using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Viora.Services;

namespace Viora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AudioController : ControllerBase
    {
        private readonly MessageHandlingService _messageHandlingService;

        public AudioController(MessageHandlingService messageHandlingService)
        {
            _messageHandlingService = messageHandlingService;
        }


        /// <summary>
        /// Streams the audio file for a specific message.
        /// </summary>
        /// <remarks>
        /// Workflow: messageId → resolve audio path → validate that the file exists → stream audio
        ///
        /// Request:
        /// - Method: GET
        /// - Route: /GetAudio/{messageId}
        /// - Params:
        ///     • messageId (int): Required. The unique identifier of the message.
        ///
        /// Response (audio/wav):
        /// - 200 OK:
        ///     • Returns the audio file as a streamed response.
        ///     • Supports range processing (seek / partial loading).
        ///
        /// - 404 NotFound:
        ///     • Message not found or has no audio.
        ///     • Audio file missing on disk.
        ///
        /// Notes:
        /// - The response is binary (not JSON) and can be played directly in browsers or audio players.
        /// - enableRangeProcessing allows efficient streaming and seeking without downloading the entire file.
        /// </remarks>
        [HttpGet("GetAudio/{messageId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("audio/wav")]
        public async Task<IActionResult> GetAudio(int messageId)
        {
            var fullPath = await _messageHandlingService.GetAudioUrl(messageId);

            if (string.IsNullOrEmpty(fullPath))
                return NotFound("Message not found or has no audio.");

            if (!System.IO.File.Exists(fullPath))
                return NotFound("Audio file not found on disk.");

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(stream, "audio/wav", enableRangeProcessing: true);
        }
    }
}
