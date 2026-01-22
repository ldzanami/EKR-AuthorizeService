using Confluent.Kafka;
using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_Shared;
using EKR_Shared.Auth.Post.Incoming;
using EKR_Shared.Handlers.Interfaces;
using EKR_Shared.Services.Interfaces.Encryption;
using EKR_Shared.Services.Interfaces.Infrastructure;
using Serilog;
using System.Text;
using System.Text.Json;

namespace EKR_AuthorizeService.Handlers
{
    public class KafkaMessageHandler(IEnumerable<ICommandHandler> handlers,
                                     IKafkaProducerService kafkaProducerService,
                                     IRSAEncryptorService RSADecryptorService,
                                     IAESEncryptorService AESEncryptorService) : IKafkaMessageHandler<string, string>
    {
        private readonly IDictionary<string, ICommandHandler> _handlers = handlers.ToDictionary(h => h.CommandType);
        private readonly IKafkaProducerService _kafkaProducerService = kafkaProducerService;
        private readonly IRSAEncryptorService _RSADecryptorService = RSADecryptorService;
        private readonly IAESEncryptorService _AESEncryptorService = AESEncryptorService;

        public async Task HandleAsync(Message<string, string> message, CancellationToken ct)
        {

            var package = JsonSerializer.Deserialize<GeneralPackageTemplate>(message.Value)!;

            var decrypted = _RSADecryptorService.Decrypt(
                package.AESKey,
                package.IV,
                package.Content);

            try
            {
                if (!_handlers.TryGetValue(package.Type, out var handler))
                    throw new InvalidOperationException($"Unknown command: {package.Type}");

                var result = await handler.HandleAsync(
                Encoding.UTF8.GetBytes(decrypted.Content),
                message.Key,
                ct);

                await SendToKafka(
                    result,
                    message.Key,
                    decrypted.AesKey,
                    package.IV);
            }
            catch(InvalidOperationException ex)
            {
                await _kafkaProducerService.GiveAnswerAsync(new { ex.Message, Type = ex.GetType() }.ToString()!, partition: message.Key);
                throw;
            }
        }

        private async Task SendToKafka(object result, string requestId, byte[] aesKey, byte[] IV)
        {
            var answer = _AESEncryptorService.Encrypt(JsonSerializer.Serialize(result), aesKey, IV);
            await _kafkaProducerService.GiveAnswerAsync(JsonSerializer.Serialize(answer), partition: requestId);
        }
    }
}
