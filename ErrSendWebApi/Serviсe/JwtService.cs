using ErrSendApplication.Common.Configs;
using ErrSendWebApi.ModelsDto;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ErrSendWebApi.Serviсe
{
    public interface IJwtService
    {
        string GenerateToken(string username);
        string GenerateCustomToken(GenerateTokenRequest request);
    }

    public class JwtService : IJwtService
    {
        private readonly TokenConfig tokenConfig;

        public JwtService(IOptions<TokenConfig> tokenConfig)
        {
            this.tokenConfig = tokenConfig.Value;
        }

        public string GenerateToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenConfig.TokenKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: tokenConfig.Issuer,
                audience: tokenConfig.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(tokenConfig.ExpiryInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateCustomToken(GenerateTokenRequest request)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenConfig.TokenKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Додаємо ролі, якщо вони є
            if (request.Roles != null)
            {
                foreach (var role in request.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            // Додаємо користувацькі клейми, якщо вони є
            if (request.CustomClaims != null)
            {
                foreach (var claim in request.CustomClaims)
                {
                    claims.Add(new Claim(claim.Key, claim.Value));
                }
            }

            var token = new JwtSecurityToken(
                issuer: tokenConfig.Issuer,
                audience: tokenConfig.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(request.ExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
} 