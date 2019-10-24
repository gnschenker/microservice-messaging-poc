using System;
using System.Threading;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using messages;

namespace consumer
{
    class Program
    {
        static readonly string KAFKA_HOST = "kafka:9092";
        static readonly string SCHEMA_REGISTRY_URL = "http://schema-registry:8081";
        static readonly string TOPIC_NAME = "test-topic";
        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            
            Console.WriteLine("Consuming messages...");
            Console.WriteLine(" Press CTRL-c to exit.");
            Console.CancelKeyPress += (sender, args) =>
            {
                Console.WriteLine("Exiting...");
                cts.Cancel();
            };
            Consuming(cts);
        }

        private static void Consuming(CancellationTokenSource cts)
        {
            using (var consumer = GetConsumer())
            {
                consumer.Subscribe(TOPIC_NAME);
                try
                {
                    while (true)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cts.Token);

                            Console.WriteLine($"message key: {consumeResult.Message.Key}, message text: {consumeResult.Value.Text}");
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Consume error: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    consumer.Close();
                }
            }
        }

        private static IConsumer<string,TextMessage> GetConsumer()
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = KAFKA_HOST,
                GroupId = "demo-consumer-group"
            };
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                SchemaRegistryUrl = SCHEMA_REGISTRY_URL,
                SchemaRegistryRequestTimeoutMs = 5000,
                SchemaRegistryMaxCachedSchemas = 10
            };
            var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);
            var consumer =
                new ConsumerBuilder<string, TextMessage>(consumerConfig)
                    .SetKeyDeserializer(new AvroDeserializer<string>(schemaRegistry).AsSyncOverAsync())
                    .SetValueDeserializer(new AvroDeserializer<TextMessage>(schemaRegistry).AsSyncOverAsync())
                    .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
                    .Build();
            return consumer;
        }
    }
}
