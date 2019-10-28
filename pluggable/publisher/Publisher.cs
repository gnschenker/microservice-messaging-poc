using System;
using System.Threading;
using System.Threading.Tasks;
using messages;

namespace publisher
{
    class Publisher
    {
        private static readonly AutoResetEvent _closing = new AutoResetEvent(false);
        private static readonly Random rnd = new Random((int)DateTime.Now.Ticks);
        static void Main(string[] args)
        {
            Task.Factory.StartNew(() =>
            {
                Publishing(args);
            });
            Console.WriteLine("Publishing messages...");
            Console.WriteLine(" Press CTRL-c to exit.");
            Console.CancelKeyPress += (sender, args) =>
            {
                Console.WriteLine("Exit");
                _closing.Set();
            };
            _closing.WaitOne();
        }

        private static void Publishing(string[] args)
        {
            Console.WriteLine(">>>START Publishing");
            using (var strategy = GetStrategy(args))
            {
                strategy.Initialize();
                while (true)
                {
                    var key = $"key-{rnd.Next(10) + 1}";    // currently only used with Kafka
                    var message = GetRandomMessage();
                    strategy.Publish(key, message);
                    Thread.Sleep(500);
                }
            }
        }

        private static IPublisherStrategy<string, TextMessage> GetStrategy(string[] args)
        {
            var value = args.Length == 0 ? "RabbitMQ" : args[0];
            IPublisherStrategy<string, TextMessage> strategy;
            switch (value)
            {
                case "RabbitMQ":
                    strategy = new RabbitMqPublisherStrategy<string, TextMessage>();
                    break;
                case "EasyNetQ":
                    strategy = new EasyNetQPublisherStrategy<string, TextMessage>();
                    break;
                case "Kafka":
                    strategy = new KafkaPublisherStrategy<string, TextMessage>();
                    break;
                default:
                    throw new ArgumentException($"Unknown publisher strategy: {value}");
            }
            Console.WriteLine($"Using strategy '{value}'");
            return strategy;
        }

        private static TextMessage GetRandomMessage()
        {
            var points = new String('.', rnd.Next(20));
            var message = new TextMessage
            {
                Text = $"Hello Pluggable World{points}"
            };
            return message;
        }
    }
}