using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public class ReceiverUtils
    {
        public static IncomingMessage Receive(IncomingMessage message, byte[] buffer, ref int offset, ref int length)
        {
            if (message.HeaderBytesReceived < IncomingMessage.HeaderSize)
            {
                if (length > 1)
                {
                    if (message.HeaderBytesReceived == 0)
                    {
                        message.Header.Byte0 = buffer[offset + 0];
                        message.Header.Byte1 = buffer[offset + 1];
                        message.HeaderBytesReceived += 2;

                        offset += 2;
                        length -= 2;
                    }
                    else if (message.HeaderBytesReceived == 1)
                    {
                        message.Header.Byte1 = buffer[offset + 0];
                        message.HeaderBytesReceived += 1;

                        offset += 1;
                        length -= 1;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else if (length == 1)
                {
                    if (message.HeaderBytesReceived == 0)
                    {
                        message.Header.Byte0 = buffer[offset + 0];
                    }
                    else if (message.HeaderBytesReceived == 1)
                    {
                        message.Header.Byte1 = buffer[offset + 0];
                    }
                    else
                    {
                        throw new Exception();
                    }

                    message.HeaderBytesReceived += 1;
                    offset += 1;
                    length -= 1;
                }
                else
                {
                    throw new Exception();
                }

                if (message.HeaderBytesReceived == 2)
                {
                    if (message.Header.Short > message.BufferSize)
                    {
                        message = new IncomingMessage(null, 0, 0, message.Header.Short, new byte[message.Header.Short]);
                        message.Header.Short = (ushort)message.BufferSize;
                        message.HeaderBytesReceived = 2;
                    }

                    message.Length = message.Header.Short;
                    message.BytesRemaining = message.Header.Short;
                }
            }

            if (length > 0)
            {
                if (length >= message.BytesRemaining)
                {
                    System.Buffer.BlockCopy(
                        buffer,
                        offset,
                        message.BufferHandle,
                        message.BufferOffset + message.BytesRead,
                        message.BytesRemaining
                    );

                    offset += message.BytesRemaining;
                    length -= message.BytesRemaining;

                    message.BytesRead += message.BytesRemaining;
                    message.BytesRemaining -= message.BytesRemaining;
                }
                else
                {
                    System.Buffer.BlockCopy(
                        buffer,
                        offset,
                        message.BufferHandle,
                        message.BufferOffset + message.BytesRead,
                        length
                    );

                    message.BytesRead += length;
                    message.BytesRemaining -= length;

                    offset += length;
                    length -= length;
                }

                message.IsDone = message.BytesRemaining == 0;
            }

            return message;
        }
    }
}
