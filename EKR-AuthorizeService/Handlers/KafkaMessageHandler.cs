using Confluent.Kafka;
using EKR_Shared;
using EKR_Shared.Exceptions;
using EKR_Shared.Handlers.Interfaces;
using EKR_Shared.Services.Interfaces.Encryption;
using EKR_Shared.Services.Interfaces.Helpers;
using EKR_Shared.Services.Interfaces.Infrastructure;
using System.Text.Json;

namespace EKR_AuthorizeService.Handlers
{
    public class KafkaMessageHandler(IEnumerable<IPostCommandHandler> postHandlers,
                                     IKafkaProducerService kafkaProducerService,
                                     IRSAEncryptorService RSADecryptorService,
                                     IAESEncryptorService AESEncryptorService,
                                     IEnumerable<IGetCommandHandler> getHandlers,
                                     IHashCheckingService hashCheckingService) : IKafkaMessageHandler<string, string>
    {
        private readonly IDictionary<string, IPostCommandHandler> _postHandlers = postHandlers.ToDictionary(h => h.CommandType);
        private readonly IDictionary<string, IGetCommandHandler> _getHandlers = getHandlers.ToDictionary(h => h.CommandType);
        private readonly IKafkaProducerService _kafkaProducerService = kafkaProducerService;
        private readonly IRSAEncryptorService _RSADecryptorService = RSADecryptorService;
        private readonly IAESEncryptorService _AESEncryptorService = AESEncryptorService;
        private readonly IHashCheckingService _hashCheckingService = hashCheckingService;

        public async Task HandleAsync(Message<string, string> message, CancellationToken ct)
        {
            try
            {
                var package = JsonSerializer.Deserialize<GeneralPackageTemplate>(message.Value)!;

                byte[] aesKey = [];
                string content = "";

                if (package.AESKey != null)
                {

                    await _hashCheckingService.CheckHashAsync(package.Hash, new
                                                                            {
                                                                                aesKey = package.AESKey,
                                                                                type = package.Type,
                                                                                content = package.Content,
                                                                                iv = package.IV,
                                                                                requestId = package.RequestId
                                                                            });

                    aesKey = _RSADecryptorService.Decrypt(Convert.FromBase64String(package.AESKey));

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
                    throw new InvalidOperationException($"Неизвестная команда: {package.Type}");
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
                await _kafkaProducerService.GiveAnswerAsync(new ClientSideException(ex.Message).ToString()!, partition: message.Key);
            }
            catch(Exception ex)
            {
                await _kafkaProducerService.GiveAnswerAsync(new ServerSideException(ex.Message, ex).ToString(), partition: message.Key);
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
