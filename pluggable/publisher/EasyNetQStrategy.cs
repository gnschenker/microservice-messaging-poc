using System;
using EasyNetQ;

namespace publisher
{
    public class EasyNetQStrategy<TKey,TMessage> : IPublisherStrategy<TKey,TMessage>
        where TMessage: class
    {
        static readonly string RABBIT_HOST = "rabbitmq";
        private IBus bus;
        public void Initialize()
        {
            bus = RabbitHutch.CreateBus($"host={RABBIT_HOST}");
        }

        public void Publish(TKey key, TMessage message)
        {
            bus.Publish(message);
            Console.WriteLine($"Sent '{message.ToString()}'");
        }

        public void Dispose()
        {}
    }
}