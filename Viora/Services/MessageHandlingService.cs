using Microsoft.AspNetCore.SignalR;
using Viora.Data;
using Viora.Hubs;
using Viora.Models;
using Viora.Repositories;
using static System.Net.Mime.MediaTypeNames;

namespace Viora.Services
{
    public class MessageHandlingService
    {
        private readonly GenericRepository<ChatMessage> _messageRepository;
        private readonly ChatHandlingService _chatHandling;
        private readonly NotificationService _notificationService;

        public MessageHandlingService(GenericRepository<ChatMessage> messageRepository ,ChatHandlingService chatHandling, NotificationService notificationService) { 
        
            _messageRepository = messageRepository;
            _chatHandling = chatHandling;
            _notificationService = notificationService;
        }



        public async Task<int> SaveUserMessage(string message ,int userId ,int? chatId) {

            if (! chatId.HasValue || chatId==0)
            {
                string title = _chatHandling.GenerateChatTitle(message);
                chatId = await _chatHandling.CreateChat(userId ,title);
                await _notificationService.NewChatAdded(userId, chatId.Value, title);
            }

            ChatMessage newMessage = new ChatMessage()
            {
                ChatId = chatId.Value ,
                Content = message,
                SenderType = "User",
                UserId = userId

            };

            await _messageRepository.Add(newMessage);
            await _messageRepository.SaveChanges();

            return chatId.Value;
        }




    }
}
