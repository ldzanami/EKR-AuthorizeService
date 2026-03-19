namespace EKR_AuthorizeService.Services.Interfaces.Encription
{
    /// <summary>
    /// Интерфейс сервиса для генерации криптографически стойких данных (соли, ключей).
    /// </summary>
    public interface IGeneratorService
    {
        /// <summary>
        /// Генерирует криптографически стойкую соль.
        /// </summary>
        /// <param name="size">Длина соли в байтах.</param>
        /// <returns>Массив байт, содержащий соль.</returns>
        byte[] GenerateSalt(int size);
    }
}
