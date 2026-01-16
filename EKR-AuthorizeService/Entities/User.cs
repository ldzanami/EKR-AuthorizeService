using System.ComponentModel.DataAnnotations;

namespace AuthorizeService.Api.Entities
{
    /// <summary>
    /// Сущность пользователя.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Id пользователя.
        /// </summary>
        /// <remarks>Автозаполняется.</remarks>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        /// <remarks>Максимум 50 символов.</remarks>
        [MaxLength(50)]
        public required string Username { get; set; }

        /// <summary>
        /// Имя пользователя в верхнем регистре.
        /// </summary>
        public required string UsernameNormalized { get; set; }

        /// <summary>
        /// Хеш пароля пользователя.
        /// </summary>
        public required byte[] PasswordHash { get; set; }

        /// <summary>
        /// Соль для унификации пароля.
        /// </summary>
        public required byte[] Salt { get; set; }

        /// <summary>
        /// Дата создания пользователя.
        /// </summary>
        /// <remarks>Автозаполняется.</remarks>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Активные сессии пользователя.
        /// </summary>
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}