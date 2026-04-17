using Microsoft.EntityFrameworkCore;
using Viora.Dtos;
using Viora.Models;
using Viora.Repositories;

namespace Viora.Services
{
    public class ChatHandlingService
    {
        private readonly ChatRepository _chatRepository;

        public ChatHandlingService(ChatRepository chatRepository){
        
            _chatRepository = chatRepository;
        }

        public string GenerateChatTitle(string message) {

            if (string.IsNullOrWhiteSpace(message)) return "New Chat";
            return message.Length > 40 ? message.Substring(0, 37) + "..." : message;

        }

        public async Task<int> CreateChat(int userId, string title)
        {
            Chat newChat = new Chat()
            {
                UserId = userId,
                Title = title
            };

            await _chatRepository.Add(newChat);
            await _chatRepository.SaveChanges();

            return newChat.Id;
        }



        public async Task<IEnumerable<GetChatsDTO>> GetUserChats(int userId)
        {
            return await _chatRepository.GetChatsByUser(userId);
        }



        
    }
}
