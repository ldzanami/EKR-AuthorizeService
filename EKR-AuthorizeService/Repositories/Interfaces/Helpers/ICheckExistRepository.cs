namespace EKR_AuthorizeService.Repositories.Interfaces.Helpers
{
    /// <summary>
    /// Интерфейс вспомогательного репозитория, который проверяет наличие объектов в БД.
    /// </summary>
    public interface ICheckExistRepository
    {
        /// <summary>
        /// Асинхронно проверяет наличие пользователя в БД.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <exception cref="KeyNotFoundException">В случае если пользователь в БД не найден.</exception>
        Task IsUserExist(Guid userId);


        /// <summary>
        /// Асинхронно проверяет наличие сессии в БД.
        /// </summary>
        /// <param name="sessionId">Id сессии.</param>
        /// <exception cref="KeyNotFoundException">В случае если сессия в БД не найдена.</exception>
        Task IsSessionExist(Guid sessionId);
    }
}
