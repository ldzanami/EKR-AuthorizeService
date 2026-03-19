namespace EKR_AuthorizeService.Services.Interfaces.Encription
{
    /// <summary>
    /// Интерфейс сервиса для хеширования паролей.
    /// </summary>
    public interface IPasswordHashService
    {
        /// <summary>
        /// Хеширует пароль с солью через Argon2id.
        /// </summary>
        /// <param name="password">Пароль в открытом виде</param>
        /// <param name="salt">Соль (32 байта)</param>
        /// <returns>Хеш пароля в виде строки</returns>
        public byte[] HashPassword(string password, byte[] salt);

        /// <summary>
        /// Проверяет правильный ли пароль.
        /// </summary>
        /// <param name="password">Пароль пользователя.</param>
        /// <param name="hash">Хеш пароля пользователя.</param>
        /// <returns>true если пароль верен; иначе false.</returns>
        bool VerifyPassword(string password, byte[] hash);
    }
}
