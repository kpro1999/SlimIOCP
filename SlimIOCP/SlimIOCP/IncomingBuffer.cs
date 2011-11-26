using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    internal class IncomingBuffer : MessageBuffer
    {
        internal Connection Connection;

        internal readonly Peer Peer;
        internal readonly BufferManager BufferManager;
        internal readonly SocketAsyncEventArgs AsyncArgs;

        internal IncomingBuffer(Peer peer, SocketAsyncEventArgs asyncArgs, BufferManager bufferManager, int bufferId, int bufferOffset, int bufferSize)
        {
            Peer = peer;
            AsyncArgs = asyncArgs;
            BufferManager = bufferManager;
            SetBuffer(asyncArgs.Buffer, bufferId, bufferOffset, bufferSize);
        }

        internal void Reset()
        {
            Connection = null;
        }
    }
}
