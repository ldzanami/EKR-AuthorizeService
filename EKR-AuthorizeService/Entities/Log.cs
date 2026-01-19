namespace EKR_AuthorizeService.Entities
{
    /// <summary>
    /// Сущность лога.
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Id лога.
        /// </summary>
        /// <remarks>Автозаполняется.</remarks>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Id пользователя, вызвавшего лог.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Действие пользователя.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Дата выполнения.
        /// </summary>
        /// <remarks>Автозаполняется.</remarks>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ссылка на пользователя.
        /// </summary>
        public User User { get; set; }
    }
}
