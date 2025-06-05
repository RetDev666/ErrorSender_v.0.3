using System.ComponentModel.DataAnnotations;

namespace ErrSendApplication.DTO
{
    /// <summary>
    /// DTO для логіну користувача
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Ім'я користувача або email
        /// </summary>
        [Required(ErrorMessage = "Username є обов'язковим")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Пароль користувача
        /// </summary>
        [Required(ErrorMessage = "Password є обов'язковим")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Відповідь з JWT токеном
    /// </summary>
    public class TokenResponseDto
    {
        /// <summary>
        /// JWT Access токен
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh токен для оновлення
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Час закінчення дії токена (UTC)
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Тип токена (завжди "Bearer")
        /// </summary>
        public string TokenType { get; set; } = "Bearer";
    }

    /// <summary>
    /// DTO для оновлення токена
    /// </summary>
    public class RefreshTokenDto
    {
        /// <summary>
        /// Refresh токен
        /// </summary>
        [Required(ErrorMessage = "RefreshToken є обов'язковим")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO для створення API ключа
    /// </summary>
    public class CreateApiKeyDto
    {
        /// <summary>
        /// Назва API ключа
        /// </summary>
        [Required(ErrorMessage = "Name є обов'язковим")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Дата закінчення дії (опціонально)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Відповідь з API ключем
    /// </summary>
    public class ApiKeyResponseDto
    {
        /// <summary>
        /// API ключ
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Назва ключа
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Дата створення
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата закінчення дії
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
} 