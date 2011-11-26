using System.Collections.Generic;

namespace SlimIOCP
{
    internal abstract class MessageBufferProducer<T> where T : MessageBuffer
    {
        readonly Queue<T> pool;

        public MessageBufferProducer()
        {
            pool = new Queue<T>();
        }

        protected abstract T Create();
        //protected abstract void SetPeer(object peer);

        public T Get()
        {
            if (pool.Count > 0)
            {
                lock (pool)
                {
                    if (pool.Count > 0)
                    {
                        return pool.Dequeue();
                    }
                }
            }

            return Create();
        }

        public void Return(T message)
        {
            if (pool.Count < 64)
            {
                lock (pool)
                {
                    if (pool.Count < 64)
                    {
                        pool.Enqueue(message);
                    }
                }
            }
        }
    }
}
