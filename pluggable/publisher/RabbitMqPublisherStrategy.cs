using System;
using RabbitMQ.Client;
using messages;

namespace publisher
{
    public class RabbitMqPublisherStrategy<TKey,TMessage> : IPublisherStrategy<TKey,TMessage>
        where TMessage: class
    {
        static readonly string RABBIT_HOST = "rabbitmq";
        static readonly string QUEUE_NAME = "demo-queue";
        private ConnectionFactory factory;
        private IConnection connection;
        private IModel channel;

        public void Initialize()
        {
            factory = new ConnectionFactory() { HostName = RABBIT_HOST };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(queue: QUEUE_NAME,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public void Publish(TKey key, TMessage message)
        {
            var body = ByteArraySerializer<TMessage>.Serialize(message);
            channel.BasicPublish(exchange: "",
                                 routingKey: QUEUE_NAME,
                                 basicProperties: null,
                                 body: body);
            Console.WriteLine($"Sent '{message.ToString()}'");
        }

        public void Dispose()
        {
            if(channel != null)
                channel.Dispose();
            if(connection != null)
                connection.Dispose();
        }
    }
}