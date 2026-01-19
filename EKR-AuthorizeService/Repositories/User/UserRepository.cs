using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
using EKR_AuthorizeService.Data;
using EKR_AuthorizeService.Repositories.Interfaces.User;

namespace EKR_AuthorizeService.Repositories.User
{
    /// <summary>
    /// Репозиторий для работы с пользователем
    /// </summary>
    /// <param name="context">Контекст БД приложения.</param>
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        private readonly AppDbContext _context = context;

        /// <summary>
        /// Асинхронно добавляет нового пользователя в базу данных.
        /// </summary>
        /// <param name="user">Сущность пользователя.</param>
        public async Task CreateUserAsync(Entities.User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Асинхронно получает пользователя по имени.
        /// </summary>
        /// <param name="username">Имя пользователя.</param>
        /// <returns>Пользователь, если найден; иначе null.</returns>
        public async Task<Entities.User?> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.UsernameNormalized == username.ToUpper());

            return user;
        }

        /// <summary>
        /// Асинхронно получает пользователя по Id.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <returns>Пользователь, если найден; иначе null.</returns>
        public async Task<Entities.User?> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);

            return user;
        }

        /// <summary>
        /// Проверяет правильный ли пароль.
        /// </summary>
        /// <param name="password">Пароль пользователя.</param>
        /// <param name="hash">Хеш пароля пользователя.</param>
        /// <returns>true если пароль верен; иначе false.</returns>
        public bool VerifyPassword(string password, byte[] hash)
        {
            return Argon2.Verify(System.Text.Encoding.UTF8.GetString(hash), password);
        }
    }
}
