using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using messages;

namespace consumer
{
    class Consumer
    {
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
                Console.WriteLine("Exiting...");
                _closing.Set();
            };
            _closing.WaitOne();
        }

        private static void Consuming()
        {
            Console.WriteLine(">>> Start consuming!");
            using (var bus = RabbitHutch.CreateBus($"host={RABBIT_HOST}"))
            {
                bus.Subscribe<TextMessage>("test", message => {
                    Console.WriteLine("Got message: {0}", message.Text);
                });
                _closing.WaitOne();
            }
            Console.WriteLine("<<< End consuming!");
        }
    }
}
