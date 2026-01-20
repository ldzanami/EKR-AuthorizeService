using EKR_Shared;

namespace EKR_AuthorizeService.Services.Interfaces.Infrastructure
{
    /// <summary>
    /// Интерфейс сервиса для отправки ответов в Kafka.
    /// </summary>
    public interface IKafkaProducerService
    {
        /// <summary>
        /// Асинхронно отправляет ответ в выбранный partition.
        /// </summary>
        /// <param name="partition">Выбранный partition.</param>
        /// <param name="answer">Ответ от сервиса.</param>
        Task GiveAnswerToPartitionAsync(GeneralPartitionsEnum partition, GeneralPackageTemplate answer);
    }
}
