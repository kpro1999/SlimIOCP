using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    class IncomingBuffer2 : MessageBuffer2
    {
        internal Connection Connection;

        internal readonly Peer Peer;
        internal readonly SocketAsyncEventArgs AsyncArgs;

        internal IncomingBuffer2(Peer peer, SocketAsyncEventArgs asyncArgs)
        {
            Peer = peer;
            AsyncArgs = asyncArgs;
        }

        internal override void Reset()
        {
            Connection = null;
        }

        internal override void Destroy()
        {
            Connection = null;
            AsyncArgs.SetBuffer(null, 0, 0);
        }

        internal override void BufferAssigned()
        {
            AsyncArgs.SetBuffer(BufferHandle, BufferOffset, BufferSize);
        }
    }
}
