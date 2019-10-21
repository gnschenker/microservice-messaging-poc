using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace publisher
{
    class Publisher
    {
        static readonly string QUEUE_NAME = "demo-queue";
        static readonly string RABBIT_HOST = "rabbitmq";
        private static readonly AutoResetEvent _closing = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Task.Factory.StartNew(() =>
            {
                Publishing();
            });
            Console.WriteLine("Publishing messages...");
            Console.WriteLine(" Press CTRL-c to exit.");
            Console.CancelKeyPress += (sender, args) => {
                Console.WriteLine("Exit");
                _closing.Set();
            };
            _closing.WaitOne();
        }

        private static void Publishing()
        {
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

                    while (true)
                    {
                        PublishMessage(channel);
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        private static void PublishMessage(IModel channel)
        {
            string message = "Hello World!";
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                 routingKey: QUEUE_NAME,
                                 basicProperties: null,
                                 body: body);
            Console.WriteLine(" [x] Sent {0}", message);
        }
    }
}
