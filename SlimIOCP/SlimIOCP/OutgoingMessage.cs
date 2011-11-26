using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    public class OutgoingMessage : BaseOutgoingMessage, INetworkBuffer
    {
        internal Connection Connection;

        internal readonly Peer Peer;
        internal readonly SocketAsyncEventArgs AsyncArgs;

        public int BytesTransferred
        {
            get { return AsyncArgs.BytesTransferred; }
        }

        internal OutgoingMessage(Peer peer, SocketAsyncEventArgs asyncArgs)
        {
            Peer = peer;
            AsyncArgs = asyncArgs;
        }

        internal override void Destroy()
        {
            base.Destroy();
            Connection = null;
            AsyncArgs.SetBuffer(null, 0, 0);
        }

        internal override void Reset()
        {
            base.Reset();
            Connection = null;
            BufferAssigned();
        }

        internal override void BufferAssigned()
        {
            AsyncArgs.SetBuffer(BufferHandle, BufferOffset, BufferSize);
        }

        public override bool TryQueue()
        {
            if (SendDataBuffer == null)
            {
                // Write the message length
                ShortConverter.UShort = (ushort)(SendDataBytesRemaining - 2);
                BufferHandle[BufferOffset + 0] = ShortConverter.Byte0;
                BufferHandle[BufferOffset + 1] = ShortConverter.Byte1;
            }

            lock (Connection)
            {
                if (Connection.Sending)
                {
                    Connection.SendQueue.Enqueue(this);
                }
                else
                {
                    Peer.SendAsync(this);
                }
            }

            return true;
        }
    }
}
