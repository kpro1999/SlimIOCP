using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    public class OutgoingBuffer : OutgoingMessage
    {
        internal Connection Connection;

        internal readonly Peer Peer;
        internal readonly SocketAsyncEventArgs AsyncArgs;

        internal OutgoingBuffer(Peer peer, SocketAsyncEventArgs asyncArgs, BufferManager bufferManager, int bufferId, int bufferOffset, int bufferSize)
            : base(bufferManager)
        {
            Peer = peer;
            AsyncArgs = asyncArgs;
            SetBuffer(asyncArgs.Buffer, bufferId, bufferOffset, bufferSize);
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
                    Connection.SendQueue.Enqueue(AsyncArgs);
                }
                else
                {
                    Peer.SendAsync(AsyncArgs);
                }
            }

            return true;
        }

        internal override void Reset()
        {
            base.Reset();

            Connection = null;
            AsyncArgs.SetBuffer(BufferOffset, BufferSize);
        }
    }
}
