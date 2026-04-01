using Microsoft.EntityFrameworkCore;
using EKR_AuthorizeService.Data;
using EKR_AuthorizeService.Entities;
using EKR_AuthorizeService.Repositories.Interfaces.User;

namespace EKR_AuthorizeService.Repositories.User
{
    /// <summary>
    /// Репозиторий для работы с сессиями.
    /// </summary>
    public class SessionRepository(AppDbContext appDbContext) : ISessionRepository
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        /// <summary>
        /// Асинхронно получает коллекцию сессий пользователя, кроме завершённых.
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Коллекция сессий пользователя, кроме завершённых.</returns>
        public async Task<ICollection<Session>> GetUserSessionsExcludingRevokedAsync(Guid userId)
        {
            return await _appDbContext.Sessions.Where(s => s.UserId == userId && !s.IsRevoked)
                                               .ToListAsync();
        }

        /// <summary>
        /// Асинхронно добавляет сессию.
        /// </summary>
        /// <param name="session">Сессия для добавления.</param>
        public async Task AddSessionAsync(Session session)
        {
            await _appDbContext.Sessions.AddAsync(session);
            await _appDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Асинхронно получает сессию по хешу refresh токена.
        /// </summary>
        /// <param name="refreshHash">Хеш refresh токена</param>
        /// <returns>Сессия с указанным refresh токеном.</returns>
        public async Task<Session> GetSessionByRefreshHashAsync(string refreshHash)
        {
            var session = await _appDbContext.Sessions.FirstOrDefaultAsync(s => s.RefreshToken == refreshHash);
            return session;
        }

        /// <summary>
        /// Асинхронно обновляет сессию.
        /// </summary>
        /// <param name="session">Сессия для обновления.</param>
        public async Task UpdateSessionAsync(Session session)
        {
            var s = await _appDbContext.Sessions.FindAsync(session.Id);
            _appDbContext.Sessions.Update(s);
            await _appDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Асинхронно получает сессию по Id.
        /// </summary>
        /// <param name="sessionId">Id сессии.</param>
        /// <returns>Сессия с указанным Id.</returns>
        public async Task<Session> GetSessionByIdAsync(Guid sessionId)
        {
            var session = await _appDbContext.Sessions.FindAsync(sessionId);
            return session;
        }

        /// <summary>
        /// Асинхронно получает коллекцию сессий пользвателя, кроме указанной и завершённых.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <param name="keepSessionId">Id игнорируемой сессии.</param>
        /// <returns>Коллекция сессий пользвателя, кроме указанной и завершённых.</returns>
        public async Task<ICollection<Session>> GetAllUserSessionsExcludingSpecifiedAndRevokedAsync(Guid userId, Guid keepSessionId)
        {
            return await _appDbContext.Sessions.Where(s => s.UserId == userId && s.Id != keepSessionId && !s.IsRevoked)
                                               .ToListAsync();
        }

        /// <summary>
        /// Асинхронно обновляет несколько сессий.
        /// </summary>
        /// <param name="sessions">Коллекция сессий для обновления.</param>
        public async Task UpdateSessionsRangeAsync(ICollection<Session> sessions)
        {
            _appDbContext.UpdateRange(sessions);
            await _appDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Асинхронно получает все активные сессии пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <returns>Коллекция активных сессий пользователя.</returns>
        public async Task<ICollection<Session>> GetActiveUserSessionsAsync(Guid userId)
        {
            return await _appDbContext.Sessions.Where(s => s.UserId == userId && !s.IsRevoked)
                                               .ToListAsync();
        }

        /// <summary>
        /// Асинхронно получает все сессии пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <returns>Коллекция всех сессий пользователя.</returns>
        public async Task<ICollection<Session>> GetUserSessionsAsync(Guid userId)
        {
            return await _appDbContext.Sessions.Where(s => s.UserId == userId)
                                               .ToListAsync();
        }

        /// <summary>
        /// Асинхронно удаляет сессию.
        /// </summary>
        /// <param name="session">Сесиия для удаления.</param>
        public async Task RemoveSessionAsync(Session session)
        {
            var s = await _appDbContext.Sessions.FindAsync(session.Id);
            _appDbContext.Sessions.Remove(s);
            await _appDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Асинхронно получает все сессии с данной версией ключа.
        /// </summary>
        /// <param name="keyVersion">Версия ключа RSA.</param>
        /// <returns>Коллекция сессий.</returns>
        public async Task<ICollection<Session>> GetSessionsByKeyVersionAsync(string keyVersion)
        {
            return await _appDbContext.Sessions.Where(s => s.KeyVersion == keyVersion).ToListAsync();
        }
    }
}