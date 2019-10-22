using System;

namespace publisher
{
    public interface IPublisherStrategy<TKey,TMessage> : IDisposable where TMessage: class
    {
        void Initialize();
        void Publish(TKey key, TMessage message);
    }
}