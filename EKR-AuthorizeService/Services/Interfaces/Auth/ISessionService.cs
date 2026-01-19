using EKR_AuthorizeService.Entities;

namespace EKR_AuthorizeService.Services.Interfaces.Auth
{
    /// <summary>
    /// Интерфейс сервиса управления сессиями.
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Асинхронно создаёт сессию.
        /// </summary>
        /// <param name="user">Пользователь сессии.</param>
        /// <param name="deviceInfo">Информация об устройстве.</param>
        /// <returns>access + refresh токены и id сессии.</returns>
        Task<(string AccessToken, string RefreshToken, Guid SessionId)> CreateSessionAsync(User user, string deviceInfo);

        /// <summary>
        /// Асинхронная ротация refresh: валидирует входной refresh, если ок — меняет на новый и отдаёт новую пару.
        /// </summary>
        /// <param name="incomingRefreshToken">Текущий refresh токен.</param>
        /// <returns>новые access + refresh токены и id сессии.</returns>
        Task<(string AccessToken, string RefreshToken, Guid SessionId)> RefreshAsync(string incomingRefreshToken);

        /// <summary>
        /// Асинхронный разлогин конкретной сессии.
        /// </summary>
        /// <param name="sessionId">Id сессии, которую надо прервать.</param>
        Task RevokeSessionAsync(Guid sessionId);

        /// <summary>
        /// Асинхронный разлогин всех сессий пользователя, кроме указанной.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <param name="keepSessionId">Id сессии, которую нужно оставить.</param>
        Task RevokeOtherSessionsAsync(Guid userId, Guid keepSessionId);

        /// <summary>
        /// Асинхронный разлогин всех сессий пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        Task RevokeAllSessionsAsync(Guid userId);

    }
}
