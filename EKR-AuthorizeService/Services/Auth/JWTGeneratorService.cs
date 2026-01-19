using EKR_AuthorizeService.Entities;
using EKR_AuthorizeService.Services.Interfaces.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SecureMessageManager.Api.Services.Auth
{
    /// <summary>
    /// Cервис для генерации JWT токенов
    /// </summary>
    public class JWTGeneratorService : IJWTGeneratorService
    {
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _signingKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenLifetime;

        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="configuration">For DI.</param>
        /// <exception cref="ArgumentNullException">DI не передал конфигурацию.</exception>
        /// <exception cref="InvalidOperationException">В конфигурации отсутствует ключ.</exception>
        public JWTGeneratorService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            _issuer = _configuration["Jwt:Issuer"];
            _audience = _configuration["Jwt:Audience"];
            _accessTokenLifetime = int.Parse(_configuration["Jwt:AccessTokenLifetimeMinutes"] ?? "15");
        }

        /// <summary>
        /// Генерирует краткосрочный JWT токен.
        /// </summary>
        /// <param name="user">Пользователь токена.</param>
        /// <param name="expiresAt">Когда истечёт.</param>
        /// <returns>Крткосрочный JWT токен.</returns>
        /// <exception cref="ArgumentNullException">User is null.</exception>
        public string GenerateAccessToken(User user, out DateTime expiresAt)
        {
            ArgumentNullException.ThrowIfNull(user);

            var now = DateTime.UtcNow;
            expiresAt = now.AddMinutes(_accessTokenLifetime);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UsernameNormalized),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                SigningCredentials = creds,
                Issuer = _issuer,
                Audience = _audience
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        /// <summary>
        /// Генерирует долгоживущий refresh токен.
        /// </summary>
        /// <returns>Долгоживущий refresh токен.</returns>
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            RandomNumberGenerator.Fill(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Хеширует токен.
        /// </summary>
        /// <param name="token">JWT или Refresh токен.</param>
        /// <returns>Хеш токена.</returns>
        public string HashToken(string token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hash);
        }
    }
}
