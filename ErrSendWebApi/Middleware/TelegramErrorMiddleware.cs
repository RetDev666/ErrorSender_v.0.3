using ErrSendApplication.DTO;
using ErrSendApplication.Interfaces;
using System.Text.Json;

namespace ErrSendWebApi.Middleware
{
    public class TelegramErrorMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<TelegramErrorMiddleware> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public TelegramErrorMiddleware(
            RequestDelegate next,
            ILogger<TelegramErrorMiddleware> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.next = next;
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Виникла необроблена виняткова ситуація");
                
                // Відправляємо помилку в Telegram в background
                _ = Task.Run(async () => await SendErrorToTelegramAsync(ex, context));
                
                throw; // Повторне перекидання для збереження нормального потоку обробки помилок
            }
        }

        private async Task SendErrorToTelegramAsync(Exception exception, HttpContext context)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var telegramService = scope.ServiceProvider.GetService<ITelegramService>();
                
                if (telegramService == null)
                {
                    logger.LogWarning("Telegram cервіс не зареєстровано, пропускаю сповіщення про помилку");
                    return;
                }

                var errorReport = new ErrorReportDto
                {
                    ErrorMessage = exception.Message,
                    Source = $"{context.Request.Method} {context.Request.Path}",
                    StackTrace = exception.StackTrace ?? "",
                    Severity = "Error",
                    UserId = context.User?.Identity?.Name ?? "Анонім",
                    AdditionalInfo = $"User-Agent: {context.Request.Headers.UserAgent}\nRemote IP: {context.Connection.RemoteIpAddress}"
                };

                var result = await telegramService.SendErrorAsync(new Domain.Models.ErrorReport
                {
                    ErrorMessage = errorReport.ErrorMessage,
                    Source = errorReport.Source,
                    StackTrace = errorReport.StackTrace,
                    Severity = errorReport.Severity,
                    UserId = errorReport.UserId,
                    AdditionalInfo = errorReport.AdditionalInfo,
                    Timestamp = DateTime.UtcNow
                });

                if (result.IsSuccess)
                {
                    logger.LogInformation("Повідомлення про помилку успішно відправлено в Telegram");
                }
                else
                {
                    logger.LogWarning("Не вдалося надіслати повідомлення про помилку в Telegram: {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Не вдалося надіслати повідомлення про помилку в Telegram");
            }
        }
    }
} 