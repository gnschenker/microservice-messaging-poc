using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace consumer
{
    class Consumer
    {
        static readonly string QUEUE_NAME = "demo-queue";
        static readonly string RABBIT_HOST = "rabbitmq";
        private static readonly AutoResetEvent _closing = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Task.Factory.StartNew(() =>
            {
                Consuming();
            });
            Console.WriteLine("Waiting for messages...");
            Console.WriteLine(" Press CTRL-c to exit.");
            Console.CancelKeyPress += (sender, args) => {
                Console.WriteLine("Exit");
                _closing.Set();
            };
            _closing.WaitOne();
        }

        private static void Consuming()
        {
            Console.WriteLine("Start consuming!");
            var factory = new ConnectionFactory() { HostName = RABBIT_HOST };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: QUEUE_NAME,
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine(" [x] Received {0}", message);
                    };
                    channel.BasicConsume(queue: QUEUE_NAME,
                                        autoAck: true,
                                        consumer: consumer);

                    _closing.WaitOne();
                }
            }
            Console.WriteLine("End consuming!");
        }
    }
}
