using System;
using System.Threading;

namespace consumer
{
    public interface IConsumerStrategy<TKey,TMessage> : IDisposable where TMessage: class
    {
        void Initialize();
        void Consume(CancellationTokenSource cts, Action<TKey, TMessage> handler);
    }
}