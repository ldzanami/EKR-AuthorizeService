using Confluent.Kafka;
using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_Shared;
using EKR_Shared.Auth.Post.Incoming;
using EKR_Shared.Handlers;
using EKR_Shared.Services.Interfaces.Encryption;
using EKR_Shared.Services.Interfaces.Infrastructure;
using Serilog;
using System.Text;
using System.Text.Json;

namespace EKR_AuthorizeService.Handlers
{
    public class KafkaMessageHandler(IAuthService authService,
                                     IKafkaProducerService kafkaProducerService,
                                     IRSAEncryptorService RSADecryptorService,
                                     IAESEncryptorService AESEncryptorService) : IKafkaMessageHandler<string, string>
    {
        private readonly IAuthService _authService = authService;
        private readonly IKafkaProducerService _kafkaProducerService = kafkaProducerService;
        private readonly IRSAEncryptorService _RSADecryptorService = RSADecryptorService;
        private readonly IAESEncryptorService _AESEncryptorService = AESEncryptorService;

        public async Task HandleAsync(Message<string, string> message, CancellationToken ct)
        {
            var package = JsonSerializer.Deserialize<GeneralPackageTemplate>(message.Value)!;

            var decrypted = _RSADecryptorService.Decrypt(package.AESKey, package.IV, package.Content);

            if (package.Type == "register")
            {
                var result = await _authService.RegisterAsync(JsonSerializer.Deserialize<RegisterRequestDto>(decrypted.Content)!, message.Key);
                await SentToKafka(result, message.Key, decrypted.AesKey, package.IV);
            }
            else if (package.Type == "authorize")
            {
                var result = await _authService.AuthorizationAsync(JsonSerializer.Deserialize<AuthorizationDto>(decrypted.Content)!, message.Key);
                await SentToKafka(result, message.Key, decrypted.AesKey, package.IV);
            }
            else if (package.Type == "refresh")
            {
                var result = await _authService.RefreshAsync(decrypted.Content, message.Key);
                await SentToKafka(result, message.Key, decrypted.AesKey, package.IV);
            }
            else if (package.Type == "revoke")
            {
                await _authService.RevokeSessionAsync(Guid.Parse(decrypted.Content), message.Key);
                await SentToKafka("OK", message.Key, decrypted.AesKey, package.IV);
            }
            else if (package.Type == "revoke-others")
            {
                await _authService.RevokeOtherSessionsAsync(JsonSerializer.Deserialize<RevokeOtherSessionsDto>(decrypted.Content)!, message.Key);
                await SentToKafka("OK", message.Key, decrypted.AesKey, package.IV);
            }
            else if (package.Type == "revoke-all")
            {
                await _authService.RevokeAllSessionsAsync(Guid.Parse(decrypted.Content), message.Key);
                await SentToKafka("OK", message.Key, decrypted.AesKey, package.IV);
            }
            else if (package.Type == "get-active-sessions")
            {
                var result = await _authService.GetActiveUserSessionsAsync(Guid.Parse(decrypted.Content), message.Key);
                await SentToKafka(result, message.Key, decrypted.AesKey, package.IV);
            }
        }

        private async Task SentToKafka(object result, string requestId, byte[] aesKey, byte[] IV)
        {
            var answer = Encoding.UTF8.GetString(_AESEncryptorService.Encrypt(JsonSerializer.Serialize(result), aesKey, IV));
            await _kafkaProducerService.GiveAnswerAsync(answer, partition: requestId);
        }
    }
}
