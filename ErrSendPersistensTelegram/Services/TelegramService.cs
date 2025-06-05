using Domain.Models;
using ErrSendApplication.Common.Configs;
using ErrSendApplication.DTO;
using ErrSendApplication.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace ErrSendPersistensTelegram.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly IHttpClientWr _httpClient;
        private readonly TelegramConfig _config;
        private readonly ILogger<TelegramService> _logger;

        public TelegramService(
            IHttpClientWr httpClient,
            IOptions<TelegramConfig> config,
            ILogger<TelegramService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;
        }

        public async Task<SendErrorToTelegramResponse> SendErrorAsync(ErrorReport errorReport)
        {
            try
            {
                var message = FormatErrorMessage(errorReport);
                var telegramMessage = new
                {
                    chat_id = _config.ChatId,
                    text = message,
                    parse_mode = "HTML"
                };

                var json = JsonSerializer.Serialize(telegramMessage);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{_config.BaseUrl}{_config.BotToken}/sendMessage";
                
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var telegramResponse = JsonSerializer.Deserialize<TelegramApiResponse>(responseContent);
                    
                    return new SendErrorToTelegramResponse
                    {
                        IsSuccess = true,
                        Message = "Error sent successfully to Telegram",
                        TelegramMessageId = telegramResponse?.Result?.MessageId.ToString() ?? ""
                    };
                }
                else
                {
                    _logger.LogError("Failed to send message to Telegram: {Response}", responseContent);
                    return new SendErrorToTelegramResponse
                    {
                        IsSuccess = false,
                        Message = $"Telegram API error: {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending message to Telegram");
                return new SendErrorToTelegramResponse
                {
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        private string FormatErrorMessage(ErrorReport errorReport)
        {
            var emoji = errorReport.Severity switch
            {
                "Error" => "🔴",
                "Warning" => "🟡",
                "Info" => "🔵",
                _ => "⚪"
            };

            var message = new StringBuilder();
            message.AppendLine($"{emoji} <b>ERROR REPORT</b>");
            message.AppendLine($"<b>Severity:</b> {errorReport.Severity}");
            message.AppendLine($"<b>Time:</b> {errorReport.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
            message.AppendLine($"<b>Source:</b> {errorReport.Source}");
            
            if (!string.IsNullOrEmpty(errorReport.UserId))
                message.AppendLine($"<b>User:</b> {errorReport.UserId}");
            
            message.AppendLine($"<b>Message:</b> {errorReport.ErrorMessage}");
            
            if (!string.IsNullOrEmpty(errorReport.AdditionalInfo))
                message.AppendLine($"<b>Additional Info:</b> {errorReport.AdditionalInfo}");
            
            if (!string.IsNullOrEmpty(errorReport.StackTrace))
            {
                var truncatedStackTrace = errorReport.StackTrace.Length > 1000 
                    ? errorReport.StackTrace.Substring(0, 1000) + "..."
                    : errorReport.StackTrace;
                message.AppendLine($"<b>Stack Trace:</b>\n<code>{truncatedStackTrace}</code>");
            }

            return message.ToString();
        }
    }

    public class TelegramApiResponse
    {
        public bool Ok { get; set; }
        public TelegramMessage? Result { get; set; }
    }

    public class TelegramMessage
    {
        public int MessageId { get; set; }
    }
} 