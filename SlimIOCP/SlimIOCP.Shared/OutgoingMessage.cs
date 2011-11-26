using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public abstract class OutgoingMessage : MessageBuffer
    {
        internal readonly BufferManager BufferManager;
        internal ShortConverter ShortConverter;

        internal byte[] SendDataBuffer;
        internal int SendDataOffset;
        internal int SendDataBytesSent;
        internal int SendDataBytesRemaining;

        public bool HasDataToSend { get { return SendDataBytesRemaining > HEADER_SIZE; } }

        internal OutgoingMessage(BufferManager bufferManager)
        {
#if DEBUG
            if (bufferManager == null)
            {
                throw new ArgumentNullException("bufferManager");
            }
#endif

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
                    SendDataBuffer = new byte[SendDataBytesRemaining];

                    // Write the message length
                    ShortConverter.UShort = (ushort)length;
                    SendDataBuffer[0] = ShortConverter.Byte0;
                    SendDataBuffer[1] = ShortConverter.Byte1;

                    // Copy the message into the SendData buffer
                    System.Buffer.BlockCopy(data, offset, SendDataBuffer, 2, length);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal virtual void Reset()
        {
            SendDataBuffer = null;
            SendDataOffset = 0;
            SendDataBytesSent = 0;
            SendDataBytesRemaining = HEADER_SIZE;
        }

        public abstract bool TryQueue();
    }
}
