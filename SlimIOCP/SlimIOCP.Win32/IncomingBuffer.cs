using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP.Win32
{
    public class IncomingBuffer : 
        MessageBuffer, 
        INetworkBuffer<OutgoingMessage, Connection>, 
        IMessageBuffer<IncomingMessage, OutgoingMessage, Connection>
    {
        internal Connection Win32Connection;
        internal readonly SocketAsyncEventArgs AsyncArgs;

        public int BytesTransferred
        {
            get { return AsyncArgs.BytesTransferred; }
        }

        public Connection Connection
        {
            get { return Win32Connection; }
        }

        internal IncomingBuffer(SocketAsyncEventArgs asyncArgs)
        {
            AsyncArgs = asyncArgs;
        }

        internal override void Reset()
        {
            Win32Connection = null;
        }

        internal override void Destroy()
        {
            Win32Connection = null;
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
                return Win32Connection.Message;
            }
            set
            {
                Win32Connection.Message = value;
            }
        }
    }
}
