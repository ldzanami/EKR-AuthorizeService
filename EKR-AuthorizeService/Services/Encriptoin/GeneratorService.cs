using EKR_AuthorizeService.Services.Interfaces.Encription;
using System.Security.Cryptography;

namespace EKR_AuthorizeService.Services.Encriptoin
{
    /// <summary>
    /// Сервис для генерации криптографически стойких последовательностей байт.
    /// </summary>
    public class GeneratorService : IGeneratorService
    {
        /// <summary>
        /// Генерирует криптографически стойкую соль.
        /// </summary>
        /// <param name="size">Длина соли в байтах.</param>
        /// <returns>Массив байт, содержащий соль.</returns>
        public byte[] GenerateSalt(int size)
        {
            var salt = new byte[size];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        /// <summary>
        /// Генерирует RSA ключевую пару.
        /// </summary>
        /// <param name="keySize">Размер ключа в битах (2048 или 4096).</param>
        /// <returns>Кортеж, содержащий приватный ключ в бинарном виде и публичный ключ в Base64.</returns>
        public (byte[] PrivateKey, string PublicKey) GenerateRSAKeyPair(int keySize)
        {
            using var rsa = RSA.Create(keySize);

            var privateKey = rsa.ExportRSAPrivateKey();

            var publicKeyBytes = rsa.ExportRSAPublicKey();
            var publicKeyBase64 = Convert.ToBase64String(publicKeyBytes);

            return (privateKey, publicKeyBase64);
        }
    }
}
