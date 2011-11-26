using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    public class IncomingBuffer : MessageBuffer, INetworkBuffer, IMessageBuffer<IncomingMessage>
    {
        internal Connection Connection;
        internal readonly SocketAsyncEventArgs AsyncArgs;

        public int BytesTransferred
        {
            get { return AsyncArgs.BytesTransferred; }
        }

        internal IncomingBuffer(SocketAsyncEventArgs asyncArgs)
        {
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

        public IncomingMessage CurrentMessage
        {
            get
            {
                return Connection.Message;
            }
            set
            {
                Connection.Message = value;
            }
        }
    }
}
