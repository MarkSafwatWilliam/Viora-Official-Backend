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
        public ChatController(ChatHandlingService chatHandlingService) {
            _chatHandlingService = chatHandlingService;
        
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
    }
}
