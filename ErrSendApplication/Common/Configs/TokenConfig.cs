namespace ErrSendApplication.Common.Configs
{
    public class TokenConfig
    {
        public string TokenKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryInMinutes { get; set; }
    }
} 