namespace ErrSendWebApi.ModelsDto
{
    /// <summary>
    /// Модель відповіді з JWT токеном
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// JWT токен для авторизації
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
} 