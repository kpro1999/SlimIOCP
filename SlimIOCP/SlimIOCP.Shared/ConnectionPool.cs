using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    internal class ConnectionPool<TOutgoingMessage, TConnection>
        where TOutgoingMessage : OutgoingMessage
        where TConnection : Connection<TOutgoingMessage>
    {
        int allocated;

        readonly int maxPooled;
        readonly int maxAllocated;
        readonly object syncObject;
        readonly Stack<TConnection> pool;
        readonly ConnectionProducer<TOutgoingMessage, TConnection> producer;

        internal ConnectionPool(ConnectionProducer<TOutgoingMessage, TConnection> producer, int maxPooled, int maxAllocated)
        {
            this.pool = new Stack<TConnection>();
            this.producer = producer;
            this.maxPooled = maxPooled;
            this.maxAllocated = maxAllocated;
            this.syncObject = new object();

            for (var i = 0; i < maxPooled; ++i)
            {
                TConnection connection;

                if (tryAllocate(out connection))
                {
                    Push(connection);
                }
                else
                {
                    //TODO: Error
                }
            }
        }

        internal bool TryPop(out TConnection connection)
        {
            lock (syncObject)
            {
                if (pool.Count > 0)
                {
                    connection = pool.Pop();
                    connection.Reset();

                    return true;
                }
            }

            return tryAllocate(out connection);
        }

        internal void Push(TConnection connection)
        {
#if DEBUG
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (connection.SendQueue.Count == 0)
            {
                connection.Reset();

                if (pool.Count < maxPooled)
                {
                    lock (pool)
                    {
                        pool.Push(connection);
                    }
                }
                else
                {
                    --allocated;
                }
            }
#endif
            else
            {
                //TODO: Error
            }
        }

        bool tryAllocate(out TConnection connection)
        {
            if (allocated < maxAllocated)
            {
                ++allocated;
                connection = producer.Create();
                return true;
            }

            connection = null;
            return false;
        }

    }
}
