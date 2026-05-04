using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using Viora.Dtos;
using Viora.Models;
using Viora.Repositories;
using Viora.Services;

namespace Viora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatHandlingService _chatHandlingService;
        private readonly MessageHandlingService _messageHandlingService;


        public ChatController(ChatHandlingService chatHandlingService, MessageHandlingService messageHandlingService) {
            _chatHandlingService = chatHandlingService;
            _messageHandlingService = messageHandlingService;
        }


        [HttpGet("GetAllChats")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetChatsDTO>))]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetChats()
        {
            int userId = int.TryParse(
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
                out var id) ? id : 0;

            if (userId == 0)
            {
                return Unauthorized();
            }

            var chats = await _chatHandlingService.GetUserChats(userId);

            if (chats == null || !chats.Any())
            {
                return NoContent();
            }

            return Ok(chats);
        }






        /// <summary>
        /// Retrieves all audio references for a specific chat.
        /// </summary>
        /// <remarks>
        /// Workflow: chatId → fetch messages → extract audio references → return list
        ///
        /// Request:
        /// - Method: GET
        /// - Route: /GetAudioUrls/{id}
        /// - Params:
        ///     • id (int): Required. The unique identifier of the chat.
        ///
        /// Response (application/json):
        /// - 200 OK:
        ///     {
        ///         "audioPaths": [
        ///             "path-or-url-1",
        ///             "path-or-url-2"
        ///         ]
        ///     }
        ///
        /// - 404 NotFound:
        ///     Returned when no audio files are found for the given chat.
        ///
        /// Notes:
        /// - This endpoint does NOT return audio content, only references (paths/URLs).
        /// - These references should be used to fetch or stream audio via another endpoint (e.g., /GetAudio/{messageId}).
        /// </remarks>
        [HttpGet("GetAudioUrls/{id}")]
        [ProducesResponseType(typeof(IDictionary<string, IEnumerable<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChatById(int id)
        {
            var audioPaths = await _messageHandlingService.GetChatMessages(id);

            if (!audioPaths.Any())
                return NotFound();

            return Ok(new { audioPaths });
        }
    }
}
