using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    public class DataToken
    {
        internal const int HeaderSize = 2;

        internal byte[] Buffer;
        internal int BufferId;
        internal int BufferSize;
        internal int BufferOffset;
        internal ShortConverter ShortConverter;

        internal byte[] SendData;
        internal int SendDataOffset;
        internal int SendDataBytesSent;
        internal int SendDataBytesRemaining;

        internal Peer Peer;
        internal Connection Connection;
        internal BufferManager BufferManager;
        internal SocketAsyncEventArgs AsyncArgs;

        internal DataToken(Peer peer, SocketAsyncEventArgs asyncArgs, BufferManager bufferManager, int bufferId, int bufferOffset, int bufferSize)
        {
            Peer = peer;
            AsyncArgs = asyncArgs;

            Buffer = asyncArgs.Buffer;
            BufferId = bufferId;
            BufferSize = bufferSize;
            BufferOffset = bufferOffset;
            BufferManager = bufferManager;
        }

        public bool TryWrite(byte[] data)
        {
            return TryWrite(data, 0, data.Length);
        }

        public bool TryWrite(byte[] data, int offset, int length)
        {
            if (SendDataBytesRemaining + length < BufferSize)
            {
                System.Buffer.BlockCopy(data, offset, Buffer, BufferOffset + SendDataBytesRemaining, length);
                SendDataBytesRemaining += length;
                return true;
            }
            else
            {
                if (SendDataBytesRemaining == HeaderSize)
                {
                    // Calculate total message data
                    SendDataBytesRemaining += length;
                    SendData = new byte[SendDataBytesRemaining];

                    // Write the message length
                    ShortConverter.Short = (ushort)length;
                    SendData[0] = ShortConverter.Byte0;
                    SendData[1] = ShortConverter.Byte1;

                    // Copy the message into the SendData buffer
                    System.Buffer.BlockCopy(data, offset, SendData, 2, length);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool TryQueue()
        {
            if (SendData == null)
            {
                // Write the message length
                ShortConverter.Short = (ushort)(SendDataBytesRemaining - 2);
                Buffer[BufferOffset + 0] = ShortConverter.Byte0;
                Buffer[BufferOffset + 1] = ShortConverter.Byte1;
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
    }
}
