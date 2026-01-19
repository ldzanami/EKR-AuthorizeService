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

        /// <summary>
        /// Генерирует RSA ключевую пару.
        /// </summary>
        /// <param name="keySize">Размер ключа в битах (2048 или 4096).</param>
        /// <returns>Кортеж, содержащий приватный ключ в бинарном виде и публичный ключ в Base64.</returns>
        (byte[] PrivateKey, string PublicKey) GenerateRSAKeyPair(int keySize);
    }
}
