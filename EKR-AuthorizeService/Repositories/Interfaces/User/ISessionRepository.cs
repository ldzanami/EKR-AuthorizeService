using EKR_AuthorizeService.Entities;

namespace EKR_AuthorizeService.Repositories.Interfaces.User
{
    /// <summary>
    /// Интерфейс репозитория для работы с сессиями.
    /// </summary>
    public interface ISessionRepository
    {
        /// <summary>
        /// Асинхронно получает коллекцию сессий пользователя, кроме завершённых.
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Коллекция сессий пользователя, кроме завершённых.</returns>
        Task<ICollection<Session>> GetUserSessionsExcludingRevokedAsync(Guid userId);

        /// <summary>
        /// Асинхронно добавляет сессию.
        /// </summary>
        /// <param name="session">Сессия для добавления.</param>
        Task AddSessionAsync(Session session);

        /// <summary>
        /// Асинхронно получает сессию по хешу refresh токена.
        /// </summary>
        /// <param name="refreshHash">Хеш refresh токена</param>
        /// <returns>Сессия с указанным refresh токеном.</returns>
        Task<Session> GetSessionByRefreshHashAsync(string refreshHash);

        /// <summary>
        /// Асинхронно обновляет сессию.
        /// </summary>
        /// <param name="session">Сессия для обновления.</param>
        Task UpdateSessionAsync(Session session);

        /// <summary>
        /// Асинхронно получает сессию по Id.
        /// </summary>
        /// <param name="sessionId">Id сессии.</param>
        /// <returns>Сессия с указанным Id.</returns>
        Task<Session> GetSessionByIdAsync(Guid sessionId);

        /// <summary>
        /// Асинхронно получает коллекцию сессий пользвателя, кроме указанной и завершённых.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <param name="keepSessionId">Id игнорируемой сессии.</param>
        /// <returns>Коллекция сессий пользвателя, кроме указанной и завершённых.</returns>
        Task<ICollection<Session>> GetAllUserSessionsExcludingSpecifiedAndRevokedAsync(Guid userId, Guid keepSessionId);

        /// <summary>
        /// Асинхронно обновляет несколько сессий.
        /// </summary>
        /// <param name="sessions">Коллекция сессий для обновления.</param>
        Task UpdateSessionsRangeAsync(ICollection<Session> sessions);

        /// <summary>
        /// Асинхронно получает все сессии пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <returns>Коллекция всех сессий пользователя.</returns>
        Task<ICollection<Session>> GetUserSessionsAsync(Guid userId);

        /// <summary>
        /// Асинхронно удаляет сессию.
        /// </summary>
        /// <param name="session">Сесиия для удаления.</param>
        Task RemoveSessionAsync(Session session);

        /// <summary>
        /// Асинхронно получает все активные сессии пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <returns>Коллекция активных сессий пользователя.</returns>
        Task<ICollection<Session>> GetActiveUserSessionsAsync(Guid userId);
    }
}
