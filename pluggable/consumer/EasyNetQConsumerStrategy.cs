using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;

namespace consumer
{
    public class EasyNetQConsumerStrategy<TKey, TMessage> : IConsumerStrategy<TKey, TMessage>
        where TMessage: class
    {
        static readonly string RABBIT_HOST = "rabbitmq";
        private IBus bus;
        public void Initialize()
        {
            bus = RabbitHutch.CreateBus($"host={RABBIT_HOST}");
        }

        public void Consume(CancellationTokenSource cts, Action<TKey, TMessage> handler)
        {
            bus.Subscribe<TMessage>("my-subscription", message => {
                handler(default(TKey), message);
            });
            Console.WriteLine("Waiting at handle...");
            cts.Token.WaitHandle.WaitOne();
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing...");
            if(bus != null)
                bus.Dispose();
        }
    }
}