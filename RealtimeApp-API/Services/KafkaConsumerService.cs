
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using RealtimeApp_API.Hubs;

namespace RealtimeApp_API.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IHubContext<DataHub> _hubContext;
        private readonly string _kafkaTopic;
        private readonly string _kafkaBootstrapServers;
        private readonly string _consumerGroupId;

        public KafkaConsumerService(IHubContext<DataHub> hubContext)
        {
            _hubContext = hubContext;
            _kafkaTopic = "realtime-data";
            _kafkaBootstrapServers = "localhost:9092";
            _consumerGroupId = "signalr-kafka-consumer-group";
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        private void StartConsumerLoop(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                GroupId = _consumerGroupId,
                BootstrapServers = _kafkaBootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true,
                EnableAutoOffsetStore = true 
            };

            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe(_kafkaTopic);

                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(stoppingToken);
                            var message = consumeResult.Message.Value;
                            Console.WriteLine($"Received Kafka message: {message}");

                            _hubContext.Clients.All.SendAsync("ReceiveData", message).Wait();
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Error consuming message: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Kafka consumer stopped.");
                }
                finally
                {
                    consumer.Close();
                }
            }
        }
    }
}
