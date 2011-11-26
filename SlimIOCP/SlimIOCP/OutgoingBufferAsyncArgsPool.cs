using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    internal class OutgoingBufferAsyncArgsPool
    {
        readonly Peer peer;
        readonly Queue<SocketAsyncEventArgs> pool;
        readonly BufferManager bufferManager;

        public OutgoingBufferAsyncArgsPool(Peer peer)
            : this(peer, 1024, 1024, 1024)
        {

        }

        internal OutgoingBufferAsyncArgsPool(Peer peer, int preAllocateAmount, int bufferChunkSize, int chunksPerBuffer)
        {
            this.peer = peer;

            pool = new Queue<SocketAsyncEventArgs>();
            bufferManager = new BufferManager(5, bufferChunkSize, chunksPerBuffer);

            SocketAsyncEventArgs asyncArgs;

            for (var i = 0; i < preAllocateAmount; ++i)
            {
                if (TryAllocate(out asyncArgs))
                {
                    if (!TryPush(asyncArgs))
                    {
                        //TODO: Report error
                    }
                }
                else
                {
                    //TODO: Report error   
                }
            }
        }

        public bool TryPush(SocketAsyncEventArgs asyncArgs)
        {
#if DEBUG
            if (asyncArgs == null)
            {
                throw new ArgumentNullException("asyncArgs");
            }

            if (!(asyncArgs.UserToken is OutgoingBuffer))
            {
                throw new ArgumentException("UserToken is not an OutgoingMessage", "asyncArgs");
            }
#endif
            var message = (OutgoingBuffer)asyncArgs.UserToken;
            message.Reset();

            lock (pool)
            {
                pool.Enqueue(asyncArgs);
            }

            return true;
        }

        public bool TryPop(out SocketAsyncEventArgs asyncArgs)
        {
            if (pool.Count > 0)
            {
                lock (pool)
                {
                    if (pool.Count > 0)
                    {
                        asyncArgs = pool.Dequeue();
                        return true;
                    }
                }
            }

            return TryAllocate(out asyncArgs);
        }

        bool TryAllocate(out SocketAsyncEventArgs asyncArgs)
        {
            int bufferId;
            int bufferOffset;
            int bufferSize;
            byte[] bufferHandle;

            if (bufferManager.TryAllocateBuffer(out bufferId, out bufferOffset, out bufferSize, out bufferHandle))
            {
                asyncArgs = new SocketAsyncEventArgs();
                asyncArgs.SetBuffer(bufferHandle, bufferOffset, bufferSize);
                asyncArgs.UserToken = new OutgoingBuffer(peer, asyncArgs, bufferManager, bufferId, bufferOffset, bufferSize);
                asyncArgs.Completed += new EventHandler<SocketAsyncEventArgs>(peer.OnComplete);

                return true;
            }

            //TODO: Error
            asyncArgs = null;
            return false;
        }
    }
}
