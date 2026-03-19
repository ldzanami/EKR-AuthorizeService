using EKR_AuthorizeService.Repositories.Interfaces.User;
using EKR_AuthorizeService.Services.Interfaces.Encription;
using EKR_Shared.Services.Encryption;
using EKR_Shared.Services.Interfaces.Encryption;
using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace EKR_AuthorizeService.Services.Encriptoin
{
    /// <summary>
    /// Сервис ротации RSA ключей.
    /// </summary>
    public class KeysRotationService(IConfiguration configuration,
                                     ISessionRepository sessionRepository,
                                     IRSAEncryptorService RSAEncryptorService) : IKeysRotationService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ISessionRepository _sessionRepository = sessionRepository;
        private readonly IRSAEncryptorService _RSAEncryptorService = RSAEncryptorService;

        /// <summary>
        /// Плановая ротация ключей.
        /// </summary>
        public void DefaultRotation()
        {

            var dir = "keys/current";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir!);
            }

            var keyFiles = Directory.GetFiles("keys/current")
                                    .Where(f => Path.GetFileName(f) == "public.pem"
                                             || Path.GetFileName(f) == "private.pem")
                                    .ToArray();

            DateTime creationTime = DateTime.Now;

            if (keyFiles.Length > 0)
            {
                creationTime = File.GetCreationTime(keyFiles.First());
            }

            if (keyFiles.Length == 0 || creationTime.AddMonths(6) <= DateTime.Now)
            {
                if (keyFiles.Length != 0)
                {
                    var versionFolders = Directory.GetDirectories("keys/", "version*");

                    int nextVersion = 1;
                    if (versionFolders.Length > 0)
                    {
                        nextVersion = versionFolders
                                      .Select(f => Path.GetFileName(f).Replace("version", ""))
                                      .Select(n => int.TryParse(n, out int v) ? v : 0)
                                      .Max() + 1;
                    }

                    string nextVersionFolder = Path.Combine("keys/", $"version {nextVersion}");

                    _configuration["CurrentKeyVersion"] = $"version {nextVersion + 1}";

                    Directory.CreateDirectory(nextVersionFolder);

                    foreach (var file in Directory.GetFiles("keys/current"))
                    {
                        string destFile = Path.Combine(nextVersionFolder, Path.GetFileName(file));
                        File.Move(file, destFile);
                    }
                }

                Log.Information("Generate RSA keys");
                using var rsa = RSA.Create(2048);

                var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
                File.WriteAllText("keys/current/private.pem", privateKeyPem);

                var publicKeyPem = rsa.ExportSubjectPublicKeyInfoPem();
                File.WriteAllText("keys/current/public.pem", publicKeyPem);
            }
        }

        /// <summary>
        /// Выведение ключа из ротации при компрометации.
        /// </summary>
        /// <param name="keyVersion">Версия ключа.</param>
        public async Task<bool> ComprometatedRotationAsync(string keyVersion)
        {
            Log.Warning("Выведение из использования ключа {@k}", keyVersion);

            var sessions = await _sessionRepository.GetSessionsByKeyVersionAsync(keyVersion);

            var versionFolders = Directory.GetDirectories("keys/", "version*").Select(d => d.TrimStart("keys/").ToString()).ToList();

            if (keyVersion != _configuration["CurrentKeyVersion"] && versionFolders.Any(d => d == keyVersion))
            {
                foreach (var session in sessions)
                {
                    var decrAes = Encoding.UTF8.GetString(RSAEncryptorService.Decrypt(session.EncryptedAESKey, keyVersion));
                    session.EncryptedAESKey = RSAEncryptorService.Encrypt(decrAes);
                    session.KeyVersion = _configuration["CurrentKeyVersion"]!;
                }

                await _sessionRepository.UpdateSessionsRangeAsync(sessions);
                Directory.Delete($"keys/{keyVersion}", true);
                Directory.CreateDirectory($"keys/{keyVersion}");
                Log.Warning("Выведение из использования ключа {@k} Завершено", keyVersion);
                return true;
            }
            else if (keyVersion == _configuration["CurrentKeyVersion"])
            {
                foreach (var session in sessions)
                {
                    session.EncryptedAESKey = RSAEncryptorService.Decrypt(session.EncryptedAESKey, "current");
                }

                Log.Information("Generate RSA keys");
                using var rsa = RSA.Create(2048);

                var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
                File.WriteAllText("keys/current/private.pem", privateKeyPem);

                var publicKeyPem = rsa.ExportSubjectPublicKeyInfoPem();
                File.WriteAllText("keys/current/public.pem", publicKeyPem);

                foreach (var session in sessions)
                {
                    session.EncryptedAESKey = RSAEncryptorService.Encrypt(Encoding.UTF8.GetString(session.EncryptedAESKey));
                }

                await _sessionRepository.UpdateSessionsRangeAsync(sessions);
                Log.Warning("Выведение из использования ключа {@k} Завершено", keyVersion);
                return true;
            }
            else
            {
                Log.Error("Версия ключа некорректна. {@k}", keyVersion);
                return false;
            }
        }
    }
}
