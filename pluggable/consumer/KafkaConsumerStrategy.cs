using System;
using System.Threading;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace consumer
{
    public class KafkaConsumerStrategy<TKey, TMessage> : IConsumerStrategy<TKey, TMessage>
        where TMessage: class
    {
        static readonly string KAFKA_HOST = "kafka:9092";
        static readonly string SCHEMA_REGISTRY_URL = "http://schema-registry:8081";
        static readonly string TOPIC_NAME = "test-topic";
        private IConsumer<TKey,TMessage> consumer;

        public void Initialize()
        {
            consumer = GetConsumer();
            consumer.Subscribe(TOPIC_NAME);
        }

        public void Consume(CancellationTokenSource cts, Action<TKey, TMessage> handler)
        {
            try
            {
                while (true)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cts.Token);
                        handler(consumeResult.Message.Key, consumeResult.Message.Value);
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

        public void Dispose()
        {
            if(consumer != null)
            {
                consumer.Close();
                consumer.Dispose();
            }
        }

        private IConsumer<TKey,TMessage> GetConsumer()
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
                new ConsumerBuilder<TKey, TMessage>(consumerConfig)
                    .SetKeyDeserializer(new AvroDeserializer<TKey>(schemaRegistry).AsSyncOverAsync())
                    .SetValueDeserializer(new AvroDeserializer<TMessage>(schemaRegistry).AsSyncOverAsync())
                    .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
                    .Build();
            return consumer;
        }
    }
}