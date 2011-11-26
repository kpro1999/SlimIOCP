using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    internal class QueuePool<T> where T : class
    {
        readonly Stack<Queue<T>> pool = new Stack<Queue<T>>();

        public QueuePool(int preAllocateAmount)
        {
            Queue<T> queue;

            for (var i = 0; i < preAllocateAmount; ++i)
            {
                if (TryAllocate(out queue))
                {
                    if (!TryPush(queue))
                    {
                        //TODO: Error
                    }
                }
                else
                {
                    //TODO: Error
                    break;
                }
            }
        }

        public bool TryPop(out Queue<T> queue)
        {
            if (pool.Count > 0)
            {
                lock (pool)
                {
                    if (pool.Count > 0)
                    {
                        queue = pool.Pop();
                        return true;
                    }
                }
            }

            return TryAllocate(out queue);
        }

        public bool TryPush(Queue<T> queue)
        {
#if DEBUG
            if (queue == null)
            {
                throw new ArgumentNullException("queue");
            }

            if (queue.Count > 0)
            {
                throw new ArgumentException("Queue was not empty", "queue");
            }
#endif

            queue.Clear();

            lock (pool)
            {
                pool.Push(queue);
            }

            return true;
        }

        bool TryAllocate(out Queue<T> queue)
        {
            queue = new Queue<T>();
            return true;
        }
    }
}
