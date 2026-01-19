using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using EKR_AuthorizeService.Services.Interfaces.Encription;

namespace EKR_AuthorizeService.Services.Encriptoin
{
    /// <summary>
    /// Сервис для хеширования паролей.
    /// </summary>
    public class PasswordHashService : IPasswordHashService
    {
        /// <summary>
        /// Хеширует пароль с солью через Argon2id.
        /// </summary>
        /// <param name="password">Пароль в открытом виде</param>
        /// <param name="salt">Соль (32 байта)</param>
        /// <returns>Хеш пароля в виде строки</returns>
        public byte[] HashPassword(string password, byte[] salt)
        {
            var config = new Argon2Config
            {
                Type = Argon2Type.DataIndependentAddressing,
                Version = Argon2Version.Nineteen,
                TimeCost = 10,
                MemoryCost = 32768,
                Lanes = 5,
                Threads = Environment.ProcessorCount,
                Password = System.Text.Encoding.UTF8.GetBytes(password),
                Salt = salt,
                HashLength = 32
            };

            using var argon2 = new Argon2(config);
            string hashString;
            using (SecureArray<byte> hashA = argon2.Hash())
            {
                hashString = config.EncodeString(hashA.Buffer);
            }
            return System.Text.Encoding.UTF8.GetBytes(hashString);
        }
    }
}
