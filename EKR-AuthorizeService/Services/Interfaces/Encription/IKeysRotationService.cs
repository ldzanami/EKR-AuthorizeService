namespace EKR_AuthorizeService.Services.Interfaces.Encription
{
    /// <summary>
    /// Интерфейс сервиса ротации RSA ключей.
    /// </summary>
    public interface IKeysRotationService
    {
        /// <summary>
        /// Плановая ротация ключей.
        /// </summary>
        void DefaultRotation();

        /// <summary>
        /// Выведение ключа из ротации при компрометации.
        /// </summary>
        /// <param name="keyVersion">Версия ключа.</param>
        Task<bool> ComprometatedRotationAsync(string keyVersion);
    }
}
