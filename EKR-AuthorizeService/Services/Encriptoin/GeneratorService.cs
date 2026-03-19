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
    }
}
