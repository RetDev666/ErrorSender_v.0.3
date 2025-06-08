using System.ComponentModel.DataAnnotations;

namespace ErrSendWebApi.ModelsDto
{
    /// <summary>
    /// Модель для генерації тестового JWT токена
    /// </summary>
    public class GenerateTokenRequest
    {
        /// <summary>
        /// Ім'я користувача для токена
        /// </summary>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Додаткові ролі користувача (необов'язково)
        /// </summary>
        public List<string>? Roles { get; set; }

        /// <summary>
        /// Час життя токена в хвилинах (за замовчуванням 60)
        /// </summary>
        public int ExpiryMinutes { get; set; } = 60;

        /// <summary>
        /// Додаткові клейми (claims) для токена (необов'язково)
        /// </summary>
        public Dictionary<string, string>? CustomClaims { get; set; }
    }
} 