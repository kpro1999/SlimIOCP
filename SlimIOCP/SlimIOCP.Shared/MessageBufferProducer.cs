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
            if (pool.Count < 32)
            {
                lock (pool)
                {
                    if (pool.Count < 32)
                    {
                        pool.Enqueue(message);
                    }
                }
            }
        }
    }
}
