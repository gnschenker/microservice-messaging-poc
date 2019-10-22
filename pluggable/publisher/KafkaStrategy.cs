using System;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Confluent.Kafka.SyncOverAsync;

namespace publisher
{
    public class KafkaStrategy<TKey,TMessage> : IPublisherStrategy<TKey,TMessage>
        where TMessage: class
    {
        static readonly string KAFKA_HOST = "kafka:9092";
        static readonly string SCHEMA_REGISTRY_URL = "http://schema-registry:8081";
        static readonly string TOPIC_NAME = "test-topic";
        private CachedSchemaRegistryClient schemaRegistry;
        private IProducer<TKey,TMessage> producer;
        public void Initialize()
        {
            var config = new ProducerConfig { BootstrapServers = KAFKA_HOST };
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                SchemaRegistryUrl = SCHEMA_REGISTRY_URL,
                SchemaRegistryRequestTimeoutMs = 5000,
                SchemaRegistryMaxCachedSchemas = 10
            };
            schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);
            producer = new ProducerBuilder<TKey, TMessage>(config)
                .SetKeySerializer(new AvroSerializer<TKey>(schemaRegistry).AsSyncOverAsync())
                .SetValueSerializer(new AvroSerializer<TMessage>(schemaRegistry).AsSyncOverAsync())
                .Build();
        }

        public void Publish(TKey key, TMessage message)
        {
            var msg = new Message<TKey, TMessage>{
                Key = key,
                Value = message
            };
            try
            { 
                producer.Produce(TOPIC_NAME, msg, dr => {
                    Console.WriteLine($"Delivered key={dr.Key}, value={dr.Value} to partition {dr.Partition}");
                });
            } 
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing KafkaStrategy...");
            if(schemaRegistry != null)
                schemaRegistry.Dispose();
            if(producer != null)
                producer.Dispose();
        }
    }
}