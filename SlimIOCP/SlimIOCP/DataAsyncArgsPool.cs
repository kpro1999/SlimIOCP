using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    internal class DataAsyncArgsPool : Pool
    {
        readonly Peer peer;
        readonly Stack<SocketAsyncEventArgs> pool;
        readonly BufferManager bufferManager;

        public DataAsyncArgsPool(Peer peer)
            : this(peer, 1024, 1024, 1024)
        {

        }

        internal DataAsyncArgsPool(Peer peer, int preAllocateAmount, int bufferChunkSize, int chunksPerBuffer)
        {
            this.peer = peer;

            pool = new Stack<SocketAsyncEventArgs>();
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
                //TODO: Error
                return false;
            }

            if (!(asyncArgs.UserToken is DataToken))
            {
                //TODO: Error
                return false;
            }
#endif
            var token = (DataToken)asyncArgs.UserToken;

            token.Connection = null;
            token.SendData = null;
            token.SendDataBytesRemaining = DataToken.HeaderSize;
            token.SendDataBytesSent = 0;
            token.SendDataOffset = 0;

            asyncArgs.SetBuffer(token.BufferOffset, token.BufferSize);

            lock (pool)
            {
                pool.Push(asyncArgs);
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
                        asyncArgs = pool.Pop();
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
                Allocated++;

                asyncArgs = new SocketAsyncEventArgs();
                asyncArgs.SetBuffer(bufferHandle, bufferOffset, bufferSize);
                asyncArgs.UserToken = new DataToken(peer, asyncArgs, bufferManager, bufferId, bufferOffset, bufferSize);
                asyncArgs.Completed += new EventHandler<SocketAsyncEventArgs>(peer.OnComplete);

                return true;
            }

            //TODO: Error
            asyncArgs = null;
            return false;
        }
    }
}
