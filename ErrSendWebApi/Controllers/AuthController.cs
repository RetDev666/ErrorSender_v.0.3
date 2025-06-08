using ErrSendWebApi.ModelsDto;
using ErrSendWebApi.Serviсe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ErrSendWebApi.Controllers
{
    /// <summary>
    /// Контролер для автентифікації та управління токенами
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : BaseController
    {
        private readonly IJwtService jwtService;

        public AuthController(IJwtService jwtService)
        {
            this.jwtService = jwtService;
        }

        /// <summary>
        /// Автентифікація користувача та отримання JWT токена
        /// </summary>
        /// <param name="request">Дані для автентифікації (логін та пароль)</param>
        /// <returns>JWT токен для подальшої авторизації</returns>
        /// <response code="200">Успішна автентифікація, повертає JWT токен</response>
        /// <response code="401">Невірний логін або пароль</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public ActionResult<AuthResponse> Login([FromBody] AuthRequest request)
        {
            // For demo purposes, we're using a simple authentication
            // In production, you should validate against a database
            if (request.Username == "admin" && request.Password == "admin")
            {
                var token = jwtService.GenerateToken(request.Username);
                return Ok(new AuthResponse { Token = token });
            }

            return Unauthorized("Invalid username or password");
        }

        /// <summary>
        /// Генерація тестового JWT токена з користувацькими параметрами
        /// </summary>
        /// <param name="request">Параметри для генерації токена</param>
        /// <returns>JWT токен з вказаними параметрами</returns>
        /// <response code="200">Токен успішно згенеровано</response>
        [HttpPost("generate-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), (int)HttpStatusCode.OK)]
        public ActionResult<AuthResponse> GenerateToken([FromBody] GenerateTokenRequest request)
        {
            var token = jwtService.GenerateCustomToken(request);
            return Ok(new AuthResponse { Token = token });
        }
    }
} 