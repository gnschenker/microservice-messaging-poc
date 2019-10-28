using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using messages;

namespace consumer
{
    class Consumer
    {
        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.WriteLine("Waiting for messages...");
            Console.WriteLine(" Press CTRL-c to exit.");
            Console.CancelKeyPress += (sender, args) => {
                Console.WriteLine("Exiting...");
                cts.Cancel();
            };
            Task.Factory.StartNew(() =>
            {
                Consuming(cts, args);
            });
            cts.Token.WaitHandle.WaitOne();
            Console.WriteLine("### Exiting Application!");
        }

        private static void Consuming(CancellationTokenSource cts, string[] args)
        {
            Console.WriteLine(">>> Start consuming!");
            using(var strategy = GetStrategy(args))
            {
                strategy.Initialize();
                strategy.Consume(cts, (key, message) => {
                    Console.WriteLine("Got message: {0}", message.Text);
                });
            }
            Console.WriteLine("<<< End consuming!");
        }

        private static IConsumerStrategy<string,TextMessage> GetStrategy(string[] args)
        {
            var value = args.Length == 0 ? "RabbitMQ" : args[0];
            IConsumerStrategy<string, TextMessage> strategy;
            switch(value)
            {
                // case "RabbitMQ": 
                //     strategy = new RabbitMqConsumerStrategy<string, TextMessage>();
                //     break;
                case "EasyNetQ": 
                    strategy = new EasyNetQConsumerStrategy<string, TextMessage>();
                    break;
                case "Kafka":
                    strategy = new KafkaConsumerStrategy<string, TextMessage>();
                    break;
                default:
                    throw new ArgumentException($"Unknown consumer strategy: {value}");
            }
            Console.WriteLine($"Using strategy '{value}'");
            return strategy;
        }
    }
}
