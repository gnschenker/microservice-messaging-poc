using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using messages;

namespace publisher
{
    class Program
    {
        static readonly string KAFKA_HOST = "kafka:9092";
        static readonly string SCHEMA_REGISTRY_URL = "http://schema-registry:8081";
        static readonly string TOPIC_NAME = "test-topic";
        private static readonly AutoResetEvent _closing = new AutoResetEvent(false);
        private static readonly Random rnd = new Random((int)DateTime.Now.Ticks);
        static async Task Main(string[] args)
        {
            await Publishing();
            Console.WriteLine("Publishing messages...");
            Console.WriteLine(" Press CTRL-c to exit.");
            Console.CancelKeyPress += (sender, args) =>
            {
                Console.WriteLine("Exit");
                _closing.Set();
            };
            _closing.WaitOne();
        }

        private static async Task Publishing()
        {
            var config = new ProducerConfig { BootstrapServers = KAFKA_HOST };
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                SchemaRegistryUrl = SCHEMA_REGISTRY_URL,
                SchemaRegistryRequestTimeoutMs = 5000,
                SchemaRegistryMaxCachedSchemas = 10
            };
            using(var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig))
            using (var producer = new ProducerBuilder<string, TextMessage>(config)
                .SetKeySerializer(new AvroSerializer<string>(schemaRegistry))
                .SetValueSerializer(new AvroSerializer<TextMessage>(schemaRegistry))
                .Build())
            {
                while (true)
                {
                    await PublishMessage(producer);
                    Thread.Sleep(1000);
                }

            }

        }
        private static async Task PublishMessage(IProducer<string, TextMessage> producer)
        {
            try
            {
                var keyindex = rnd.Next(10) + 1;
                var points = new String('.', rnd.Next(20));
                var msg = new Message<string, TextMessage> 
                { 
                    Key = $"key-{keyindex}",
                    Value = new TextMessage{ Text = $"Hello Avro World{points}" } 
                };
                var dr = await producer.ProduceAsync(TOPIC_NAME, msg);
                Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }
        }

    }
}
