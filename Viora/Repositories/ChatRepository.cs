using Microsoft.EntityFrameworkCore;
using Viora.Data;
using Viora.Dtos;
using Viora.Models;

namespace Viora.Repositories
{
    public class ChatRepository : GenericRepository<Chat>
    {
        private readonly VioraDBContext _context;

        public ChatRepository(VioraDBContext context) : base(context)
        {
            _context = context; 
        }

        public async Task<IEnumerable<GetChatsDTO>> GetChatsByUser(int userId)
        {
            return await _context.Chats
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new GetChatsDTO
                {
                    ChatId = c.Id,
                    Title = c.Title,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }
    }
}
