using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    internal class IncomingBuffer2Producer : IMessageBufferProducer<IncomingBuffer2>
    {
        readonly Peer peer;
        readonly Queue<IncomingBuffer2> pool;

        public IncomingBuffer2Producer(Peer peer)
        {
            this.peer = peer;
            this.pool = new Queue<IncomingBuffer2>();
        }

        public IncomingBuffer2 Create()
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

            var asyncArgs = new SocketAsyncEventArgs();
            var buffer = new IncomingBuffer2(peer, asyncArgs);

            asyncArgs.UserToken = buffer;
            asyncArgs.Completed += new EventHandler<SocketAsyncEventArgs>(peer.OnComplete);

            return buffer;
        }

        public void Return(IncomingBuffer2 message)
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
