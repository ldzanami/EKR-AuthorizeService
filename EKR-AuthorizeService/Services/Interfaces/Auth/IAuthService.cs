using EKR_Shared.Auth.Get.Response;
using EKR_Shared.Auth.Post.Incoming;
using EKR_Shared.Auth.Post.Response;
using EKR_Shared.Auxiliary;

namespace EKR_AuthorizeService.Services.Interfaces.Auth
{
    /// <summary>
    /// Интерфейс сервиса для управления регистрацией и авторизацией пользователей.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Асинхронно регистрирует нового пользователя в системе.
        /// </summary>
        /// <param name="dto"> Данные для регистрации пользователя. </param>
        /// <param name="requestId">Id запроса.</param>
        Task<bool> RegisterAsync(RegisterRequestDto dto, string requestId);

        /// <summary>
        /// Асинхронно авторизует пользователя и выдает JWT токен.
        /// </summary>
        /// <param name="dto">Данные для авторизации пользователя.</param>
        /// <param name="requestId">Id запроса.</param>
        /// <returns>JWT токен при успешной авторизации.</returns>
        Task<AuthResponseDto> AuthorizationAsync(AuthorizationDto dto, string requestId);

        /// <summary>
        /// Асинхронное обновление токенов.
        /// </summary>
        /// <param name="incomingRefreshToken">Текущий refresh токен.</param>
        /// <param name="requestId">Id запроса.</param>
        /// <returns>Dto с новыми токенами.</returns>
        Task<RefreshDto> RefreshAsync(string incomingRefreshToken, string requestId);

        /// <summary>
        /// Асинхронный разлогин конкретной сессии.
        /// </summary>
        /// <param name="sessionId">Id сессии, которую надо прервать.</param>
        /// <param name="requestId">Id запроса.</param>
        Task<bool> RevokeSessionAsync(Guid sessionId, string requestId);

        /// <summary>
        /// Асинхронный разлогин всех сессий пользователя, кроме указанной.
        /// </summary>
        /// <param name="dto">Id пользователя + Id сессии, которую надо оставить.</param>
        /// <param name="requestId">Id запроса.</param>
        Task<bool> RevokeOtherSessionsAsync(RevokeOtherSessionsDto dto, string requestId);

        /// <summary>
        /// Асинхронный разлогин всех сессий пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <param name="requestId">Id запроса.</param>
        Task<bool> RevokeAllSessionsAsync(Guid userId, string requestId);

        /// <summary>
        /// Асинхронно получает коллекцию активных сессий пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <param name="requestId">Id запроса.</param>
        /// <returns>Коллекция активных сессий пользователя.</returns>
        Task<ICollection<GetSessionResponseDto>> GetActiveUserSessionsAsync(Guid userId, string requestId);
    }
}
