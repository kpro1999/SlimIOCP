using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public abstract class OutgoingMessage : Message
    {
        internal BufferManager BufferManager;
        internal ShortConverter ShortConverter;

        internal byte[] SendBuffer;
        internal int SendDataOffset;
        internal int SendDataBytesSent;
        internal int SendDataBytesRemaining;

        internal OutgoingMessage(BufferManager bufferManager)
        {
#if DEBUG
            if (bufferManager == null)
            {
                throw new ArgumentNullException("bufferManager");
            }
#endif
        }

        public bool TryWrite(byte[] data)
        {
            return TryWrite(data, 0, data.Length);
        }

        public bool TryWrite(byte[] data, int offset, int length)
        {
            if (SendDataBytesRemaining + length < BufferSize)
            {
                System.Buffer.BlockCopy(data, offset, BufferHandle, BufferOffset + SendDataBytesRemaining, length);
                SendDataBytesRemaining += length;
                return true;
            }
            else
            {
                if (SendDataBytesRemaining == HEADER_SIZE)
                {
                    // Calculate total message data
                    SendDataBytesRemaining += length;
                    SendBuffer = new byte[SendDataBytesRemaining];

                    // Write the message length
                    ShortConverter.Short = (ushort)length;
                    SendBuffer[0] = ShortConverter.Byte0;
                    SendBuffer[1] = ShortConverter.Byte1;

                    // Copy the message into the SendData buffer
                    System.Buffer.BlockCopy(data, offset, SendBuffer, 2, length);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public abstract bool TryQueue();
    }
}
