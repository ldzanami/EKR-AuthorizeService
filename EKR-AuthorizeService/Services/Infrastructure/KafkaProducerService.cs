using Confluent.Kafka;
using EKR_AuthorizeService.Services.Interfaces.Infrastructure;
using EKR_Shared;
using System.Text.Json;

namespace EKR_AuthorizeService.Services.Infrastructure
{
    /// <summary>
    /// Сервис для отправки ответов в Kafka.
    /// </summary>
    class KafkaProducerService : IKafkaProducerService
    {
        /// <summary>
        /// Асинхронно отправляет ответ в выбранный partition.
        /// </summary>
        /// <param name="partition">Выбранный partition.</param>
        /// <param name="answer">Ответ от сервиса.</param>
        public async Task GiveAnswerToPartitionAsync(GeneralPartitionsEnum partition, GeneralPackageTemplate answer)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "host.docker.internal:9092"
            };

            using var producer = new ProducerBuilder<string, string>(config).Build();

            var serializeAnswer = JsonSerializer.Serialize(answer);

            await producer.ProduceAsync("auth-answers", new Message<string, string>()
            {
                Key = partition.ToString(),
                Value = serializeAnswer
            });
        }
    }
}
