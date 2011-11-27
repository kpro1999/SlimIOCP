using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    internal class MetaMessagePool<TIncomingMessage, TOutgoingMessage, TConnection>
        where TOutgoingMessage : OutgoingMessage
        where TConnection : Connection<TOutgoingMessage>
        where TIncomingMessage : IncomingMessage<TOutgoingMessage, TConnection>, new()
    {
        int maxPooledAmount;
        readonly Queue<TIncomingMessage> pool;

        internal MetaMessagePool(int maxPooled)
        {
            pool = new Queue<TIncomingMessage>();
            maxPooledAmount = maxPooled;

            for (var i = 0; i < maxPooledAmount; ++i)
            {
                Push(new TIncomingMessage());
            }
        }

        internal TIncomingMessage Pop(MessageType type, TConnection connection)
        {
            TIncomingMessage msg;

            lock (pool)
            {
                if (pool.Count > 0)
                {
                    msg = pool.Dequeue();
                }
                else
                {
                    msg = new TIncomingMessage();
                }
            }

            msg.Type = type;
            msg.Connection = connection;

            return msg;
        }

        internal void Push(TIncomingMessage msg)
        {
            msg.Connection = null;

            if (pool.Count < maxPooledAmount)
            {
                lock (pool)
                {
                    pool.Enqueue(msg);
                }
            }
        }
    }
}
