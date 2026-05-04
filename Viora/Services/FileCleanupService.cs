using Microsoft.EntityFrameworkCore;
using Viora.Data;

namespace Viora.Services
{
    public class FileCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FileCleanupService> _logger;

        public FileCleanupService(IServiceScopeFactory scopeFactory, ILogger<FileCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanupOldAudioFilesAsync();
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }

        private async Task CleanupOldAudioFilesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<VioraDBContext>();

            var oldMessages = await dbContext.ChatMessages
                .Where(c => c.CreatedAt < DateTime.UtcNow.AddDays(-15)
                         && c.AudioUrl != null
                         && c.SenderType == "AI")
                .ToListAsync();

            if (oldMessages.Count == 0)
                return;

            foreach (var message in oldMessages)
            {
                try
                {
                    if (File.Exists(message.AudioUrl))
                        File.Delete(message.AudioUrl);

                    message.AudioUrl = null; 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete audio file for message {MessageId} at path {Path}",
                        message.Id, message.AudioUrl);
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}