using EKR_AuthorizeService.Entities;

namespace EKR_AuthorizeService.Services.Interfaces.Auth
{
    /// <summary>
    /// Интерфейс сервиса для генерации JWT токенов
    /// </summary>
    public interface IJWTGeneratorService
    {
        /// <summary>
        /// Генерирует краткосрочный JWT токен.
        /// </summary>
        /// <param name="user">Пользователь токена.</param>
        /// <param name="expiresAt">Когда истечёт.</param>
        /// <returns>Крткосрочный JWT токен.</returns>
        string GenerateAccessToken(User user, out DateTime expiresAt);

        /// <summary>
        /// Генерирует долгоживущий refresh токен.
        /// </summary>
        /// <returns>Долгоживущий refresh токен.</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Хеширует токен.
        /// </summary>
        /// <param name="token">JWT или Refresh токен.</param>
        /// <returns>Хеш токена.</returns>
        string HashToken(string token);
    }
}
