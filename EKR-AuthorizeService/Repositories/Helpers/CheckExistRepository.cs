using Microsoft.EntityFrameworkCore;
using EKR_AuthorizeService.Data;
using EKR_AuthorizeService.Repositories.Interfaces.Helpers;

namespace EKR_AuthorizeService.Repositories.Helpers
{
    /// <summary>
    /// Вспомогательный репозиторий, который проверяет наличие объектов в БД.
    /// </summary>
    /// <param name="appDbContext">Контекст БД приложения.</param>
    public class CheckExistRepository(AppDbContext appDbContext) : ICheckExistRepository
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        /// <summary>
        /// Асинхронно проверяет наличие пользователя в БД.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <exception cref="KeyNotFoundException">В случае если пользователь в БД не найден.</exception>
        public async Task IsUserExist(Guid userId)
        {
            if (!await _appDbContext.Users.AnyAsync(u => u.Id == userId))
            {
                throw new KeyNotFoundException($"Пользователя с таким Id: '{userId}' не существует.");
            }
        }


        /// <summary>
        /// Асинхронно проверяет наличие сессии в БД.
        /// </summary>
        /// <param name="sessionId">Id сессии.</param>
        /// <exception cref="KeyNotFoundException">В случае если сессия в БД не найдена.</exception>
        public async Task IsSessionExist(Guid sessionId)
        {
            if (!await _appDbContext.Sessions.AnyAsync(s => s.Id == sessionId))
            {
                throw new KeyNotFoundException($"Сессии с таким Id: '{sessionId}' не существует.");
            }
        }
    }
}
