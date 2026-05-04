using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Viora.AIResponses;
using Viora.Data;
using Viora.Dtos;
using Viora.Hubs;
using Viora.Models;
using Viora.Repositories;

namespace Viora.Services
{
    public class MessageHandlingService
    {
        private readonly GenericRepository<ChatMessage> _messageRepository;
        private readonly GenericRepository<ChatSummary> _summaryRepository;
        private readonly ChatHandlingService _chatHandling;
        private readonly NotificationService _notificationService;
        private readonly string _SummarizeChatEndPoint;
        private readonly IHttpClientFactory _httpClientFactory;

        public MessageHandlingService(
            GenericRepository<ChatMessage> messageRepository,
            GenericRepository<ChatSummary> summaryRepository,
            ChatHandlingService chatHandling,
            NotificationService notificationService,
            IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _messageRepository = messageRepository;
            _summaryRepository = summaryRepository;
            _chatHandling = chatHandling;
            _notificationService = notificationService;
            _SummarizeChatEndPoint = configuration["ApiSettings:SummarizeChatEndPoint"];
            _httpClientFactory = clientFactory;
        }

        public async Task<int> SaveMessage(string message, int userId, int? chatId, string senderType, string? audioUrl = null)
        {
            if (!chatId.HasValue || chatId == 0)
            {
                string title = _chatHandling.GenerateChatTitle(message);
                chatId = await _chatHandling.CreateChat(userId, title);
                await _notificationService.NewChatAdded(userId, chatId.Value, title);
            }

            ChatMessage newMessage = new ChatMessage()
            {
                ChatId = chatId.Value,
                Content = message,
                SenderType = senderType,
                UserId = userId,
                AudioUrl = audioUrl
            };

            await _messageRepository.Add(newMessage);
            await _messageRepository.SaveChanges();

            await SummerizeMessages(chatId);

            return chatId.Value;
        }

        //Chat Summary Logic:
        private async Task<string> SummerizeMessages(int? chatId)
        {
            var allMessages = await _messageRepository.GetQueryable();
            var messages = allMessages
                            .Where(c => c.ChatId == chatId && !c.IsSummarized)
                            .OrderBy(c => c.CreatedAt)
                            .ToList();

            if (messages.Count <= 15)
                return null;

            var toSummarize = messages.Take(15).ToList();
            var summarizeRequest = new
            {
                messages = toSummarize.Select(m => new { role = m.SenderType, content = m.Content }).ToList()
            };

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(_SummarizeChatEndPoint, summarizeRequest);

            if (!response.IsSuccessStatusCode)
                return null;

            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SummarizeResult>(jsonString);

            var summary = new ChatSummary
            {
                ChatId = chatId.Value,
                SummaryText = result.Summary,
                StartMessageId = toSummarize.First().Id,
                EndMessageId = toSummarize.Last().Id
            };

            await _summaryRepository.Add(summary);
            await _summaryRepository.SaveChanges();


            foreach (var msg in toSummarize)
            {
                msg.IsSummarized = true;

                if (!string.IsNullOrEmpty(msg.AudioUrl) && File.Exists(msg.AudioUrl))
                {
                    File.Delete(msg.AudioUrl);
                    msg.AudioUrl = null;
                }

                _messageRepository.Update(msg);
            }

            await _messageRepository.SaveChanges();

            return result.Status;
        }




        public async Task<IEnumerable<GetAudiosUrlDTO?>> GetChatMessages(int chatId)
        {
            var messages = await _messageRepository.GetQueryable();
             var audioPaths =  messages
                .Where(m => m.ChatId == chatId && !m.IsSummarized && m.SenderType== "AI")
                .OrderBy(m => m.CreatedAt)
                .Select(m => new GetAudiosUrlDTO
                {
                    Id = m.Id,
                    CreatedAt = m.CreatedAt,
                    Url = $"/api/Audio/GetAudio/{m.Id}",
                })
                .ToList();

            return audioPaths;
        }


        public async Task<string> GetAudioUrl(int messageId)
        {
            var message = await _messageRepository.GetById(messageId);

            if (message == null || string.IsNullOrEmpty(message.AudioUrl))
                return null;

            return message.AudioUrl;
        }
    }
}