using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using messages;

namespace cc_validator_net
{
    class Program
    {
        static readonly string KAFKA_HOST = "kafka:9092";
        static readonly string SCHEMA_REGISTRY_URL = "http://schema-registry:8081";
        static readonly string TOPIC_NAME = "cc-authorizations";
        private static Random rnd = new Random((int)DateTime.Now.Ticks);
        private static string[] providers = new []{ "VISA", "Mastercard", "AMEX", "American Express" };

        private static string[] ccNumbers = new []
        {
            "1234 5678 9012 3450",
            "1234 5678 9012 3451",
            "1234 5678 9012 3452",
            "1234 5678 9012 3453",
            "1234 5678 9012 3454",
            "1234 5678 9012 3455",
            "1234 5678 9012 3456",
            "1234 5678 9012 3457",
            "1234 5678 9012 3458",
            "1234 5678 9012 3459"
        };

        static CachedSchemaRegistryClient schemaRegistry;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            using(var producer = GetProducer()){
                while (true)
                {
                    await PublishMessage(producer);
                    Thread.Sleep(100);
                }
            }
        }
        private static async Task PublishMessage(IProducer<string, CCAuthorization> producer)
        {
            try
            {
                var message = GetRandomValues();
                var msg = new Message<string, CCAuthorization> 
                { 
                    Key = message.provider,
                    Value = message 
                };
                var dr = await producer.ProduceAsync(TOPIC_NAME, msg);
                Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }
        }

        static CCAuthorization GetRandomValues()
        {
            var provider = providers[rnd.Next(providers.Length)];
            var cc_number = ccNumbers[rnd.Next(ccNumbers.Length)];
            var status = "SUCCESS";
            if (rnd.Next(10) < 2)
            {
                status = "FAIL";
            }
            return new CCAuthorization
            {
                provider = provider,
                ccnumber = cc_number,
                status = status
            };
        }

        private static IProducer<string, CCAuthorization> GetProducer()
        {
            var config = new ProducerConfig { BootstrapServers = KAFKA_HOST };
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                SchemaRegistryUrl = SCHEMA_REGISTRY_URL,
                SchemaRegistryRequestTimeoutMs = 5000,
                SchemaRegistryMaxCachedSchemas = 10
            };
            schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);
            var producer = new ProducerBuilder<string, CCAuthorization>(config)
                .SetKeySerializer(new AvroSerializer<string>(schemaRegistry))
                .SetValueSerializer(new AvroSerializer<CCAuthorization>(schemaRegistry))
                .Build();
            return producer;
        }
    }
}
