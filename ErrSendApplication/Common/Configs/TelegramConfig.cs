namespace ErrSendApplication.Common.Configs
{
    public class TelegramConfig
    {
        public string BotToken { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.telegram.org/bot";
    }
} 