namespace EKR_AuthorizeService.Entities
{
    /// <summary>
    /// Сущность сессии.
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Id Сессии.
        /// </summary>
        /// <remarks>Автозаполняется.</remarks>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Id пользователя.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Долгоживущий токен.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Дата протухания токена.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Отозван ли токен
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// Информация о соединении.
        /// </summary>
        public string ConnectionInfo { get; set; }

        /// <summary>
        /// Дата создания сессии.
        /// </summary>
        /// <remarks>Автозаполняется.</remarks>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ссылка на пользователя.
        /// </summary>
        public User User { get; set; }
    }
}
