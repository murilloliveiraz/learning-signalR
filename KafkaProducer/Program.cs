using Confluent.Kafka;

var config = new ProducerConfig { BootstrapServers = "localhost:9092" }; // Your Kafka broker address

using (var producer = new ProducerBuilder<Null, string>(config).Build())
{
    Console.WriteLine("Kafka Producer started. Type messages and press Enter to send.");
    while (true)
    {
        Console.Write("Enter message: ");
        var text = Console.ReadLine();
        if (string.IsNullOrEmpty(text))
        {
            break;
        }

        try
        {
            var dr = await producer.ProduceAsync("realtime-data", new Message<Null, string> { Value = text });
            Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
        }
        catch (ProduceException<Null, string> e)
        {
            Console.WriteLine($"Delivery failed: {e.Error.Reason}");
        }
    }
}
Console.WriteLine("Producer shutting down.");
