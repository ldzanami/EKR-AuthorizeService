using Confluent.Kafka;

namespace EKR_AuthorizeService.Services.Infrastructure
{
    public class KafkaConsumerService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "host.docker.internal:9092",
                GroupId = "auth-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };
            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe("auth-requests");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);

                    var message = result.Message.Value;
                    Console.WriteLine($"Получено: {message}");
                    consumer.Commit();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            consumer.Close();
        }
    }
}
