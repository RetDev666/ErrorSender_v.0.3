using ErrSendApplication.DTO;
using ErrSendApplication.Interfaces;
using System.Text.Json;

namespace ErrSendWebApi.Middleware
{
    public class TelegramErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TelegramErrorMiddleware> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TelegramErrorMiddleware(
            RequestDelegate next,
            ILogger<TelegramErrorMiddleware> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                
                // Відправляємо помилку в Telegram в background
                _ = Task.Run(async () => await SendErrorToTelegramAsync(ex, context));
                
                throw; // Re-throw to maintain normal error handling flow
            }
        }

        private async Task SendErrorToTelegramAsync(Exception exception, HttpContext context)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var telegramService = scope.ServiceProvider.GetService<ITelegramService>();
                
                if (telegramService == null)
                {
                    _logger.LogWarning("TelegramService not registered, skipping error notification");
                    return;
                }

                var errorReport = new ErrorReportDto
                {
                    ErrorMessage = exception.Message,
                    Source = $"{context.Request.Method} {context.Request.Path}",
                    StackTrace = exception.StackTrace ?? "",
                    Severity = "Error",
                    UserId = context.User?.Identity?.Name ?? "Anonymous",
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
                    _logger.LogInformation("Error notification sent to Telegram successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to send error notification to Telegram: {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send error notification to Telegram");
            }
        }
    }
} 