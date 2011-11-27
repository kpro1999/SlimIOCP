using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP.Mono
{
    public class OutgoingMessage : SlimIOCP.OutgoingMessage, INetworkBuffer<OutgoingMessage, Connection>
    {
        internal Peer Peer;
        internal Connection MonoConnection;

        internal OutgoingMessage(Peer peer)
        {
            Peer = peer;
        }

        public int BytesTransferred
        {
            get;
            internal set;
        }

        public Connection Connection
        {
            get { return MonoConnection; }
        }

        internal override void BufferAssigned()
        {

        }

        public override bool TryQueue()
        {
            if (SendDataBuffer == null)
            {
                // Write the message length
                SendDataOffset = BufferOffset;
                ShortConverter.UShort = (ushort)(SendDataBytesRemaining - 2);
                BufferHandle[BufferOffset + 0] = ShortConverter.Byte0;
                BufferHandle[BufferOffset + 1] = ShortConverter.Byte1;
            }

            lock (MonoConnection)
            {
                if (MonoConnection.Sending)
                {
                    MonoConnection.SendQueue.Enqueue(this);
                }
                else
                {
                    Peer.Send(this);
                }
            }

            return true;
        }

        internal override void Reset()
        {
            base.Reset();
            MonoConnection = null;
        }
    }
}
