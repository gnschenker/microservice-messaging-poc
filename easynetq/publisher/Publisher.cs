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
        private static readonly Random rnd = new Random((int)DateTime.Now.Ticks);

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
                var key = $"key-{rnd.Next(10) + 1}";
                var points = new String('.', rnd.Next(20));
                var message = new TextMessage
                {
                    Text = $"Hello EasyNetQ World{points}"
                };
                bus.Publish(message);
                Console.WriteLine($"Sent '{message.Text}'");
                Thread.Sleep(1000);
            }
        }
    }
}
