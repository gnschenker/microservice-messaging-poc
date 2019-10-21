using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using messages;

namespace publisher
{
    class Publisher
    {
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
            var bus = RabbitHutch.CreateBus($"host={RABBIT_HOST}");
            while (true)
            {
                var message = new TextMessage{
                    Text = "Hello beautiful world!"
                };
                bus.Publish(message);
                Console.WriteLine($"Sent '{message.Text}'");
                Thread.Sleep(1000);
            }
        }
    }
}
