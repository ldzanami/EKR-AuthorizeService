using Confluent.Kafka;
using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_Shared;
using EKR_Shared.Auth.Post.Incoming;
using EKR_Shared.Handlers.Interfaces;
using EKR_Shared.Services.Encryption;
using EKR_Shared.Services.Interfaces.Encryption;
using EKR_Shared.Services.Interfaces.Infrastructure;
using Serilog;
using System.Text;
using System.Text.Json;

namespace EKR_AuthorizeService.Handlers
{
    public class KafkaMessageHandler(IEnumerable<IPostCommandHandler> postHandlers,
                                     IKafkaProducerService kafkaProducerService,
                                     IRSAEncryptorService RSADecryptorService,
                                     IAESEncryptorService AESEncryptorService,
                                     IEnumerable<IGetCommandHandler> getHandlers) : IKafkaMessageHandler<string, string>
    {
        private readonly IDictionary<string, IPostCommandHandler> _postHandlers = postHandlers.ToDictionary(h => h.CommandType);
        private readonly IDictionary<string, IGetCommandHandler> _getHandlers = getHandlers.ToDictionary(h => h.CommandType);
        private readonly IKafkaProducerService _kafkaProducerService = kafkaProducerService;
        private readonly IRSAEncryptorService _RSADecryptorService = RSADecryptorService;
        private readonly IAESEncryptorService _AESEncryptorService = AESEncryptorService;

        public async Task HandleAsync(Message<string, string> message, CancellationToken ct)
        {
            try
            {
                var package = JsonSerializer.Deserialize<GeneralPackageTemplate>(message.Value)!;
                byte[] aesKey = [];
                string content = "";

                if (package.AESKey != null)
                {
                    aesKey = _RSADecryptorService.Decrypt(Convert.FromBase64String(package.AESKey));

                    Log.Fatal($"AES: {aesKey.Length}");
                    Log.Fatal($"AES: {Convert.ToBase64String(aesKey)}");

                    content = _AESEncryptorService.Decrypt(aesKey, Convert.FromBase64String(package.IV), Convert.FromBase64String(package.Content));
                }


                object? result;
                if (_postHandlers.TryGetValue(package.Type, out var postHandler))
                {
                    result = await postHandler!.HandleAsync(content,
                                                            message.Key,
                                                            ct);
                }
                else if (_getHandlers.TryGetValue(package.Type, out var getHandler))
                {
                    result = await getHandler!.HandleAsync(message.Key, ct);
                }
                else
                {
                    throw new InvalidOperationException($"Unknown command: {package.Type}");
                }

                if (aesKey.Length > 0 && !(result is bool))
                {
                    await SendToKafka(result!,
                                      message.Key,
                                      aesKey,
                                      package.IV);
                }
                else
                {
                    await SendToKafka(result!, message.Key);
                }

               
            }
            catch(InvalidOperationException ex)
            {
                await _kafkaProducerService.GiveAnswerAsync(new { ex.Message, Type = ex.GetType() }.ToString()!, partition: message.Key);
                throw;
            }
        }

        private async Task SendToKafka(object result, string requestId, byte[] aesKey, string IV)
        {
            var answer = _AESEncryptorService.Encrypt(JsonSerializer.Serialize(result), aesKey, Convert.FromBase64String(IV));
            await _kafkaProducerService.GiveAnswerAsync(JsonSerializer.Serialize(answer), partition: requestId);
        }

        private async Task SendToKafka(object result, string requestId)
        {
            await _kafkaProducerService.GiveAnswerAsync(JsonSerializer.Serialize(result), partition: requestId);
        }
    }
}
