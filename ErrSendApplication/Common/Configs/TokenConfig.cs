namespace ErrSendApplication.Common.Configs
{
    public class TokenConfig
    {
        public string TokenKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = "ErrorSenderApi";
        public string Audience { get; set; } = "https://localhost:5001";
        public int TokenExpiryMinutes { get; set; } = 60;
        public int RefreshTokenExpiryDays { get; set; } = 7;
    }
} 