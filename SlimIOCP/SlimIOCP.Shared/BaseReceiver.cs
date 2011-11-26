using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SlimIOCP
{
    internal abstract class BaseReceiver
    {
#if DEBUG
        long messagesProcessed;
        DateTime timeStart = DateTime.Now;
        DateTime lastDisplayTime = DateTime.Now;
#endif
        internal abstract void ReceiveLoop();

        internal void Start(object threadState)
        {
            Console.WriteLine("Receiver thread started");
            ReceiveLoop();
        }

        internal IncomingMessage Receive(IncomingMessage message, byte[] buffer, ref int offset, ref int length)
        {
#if DEBUG
            if ((DateTime.Now - lastDisplayTime).TotalMilliseconds > 1000)
            {
                lastDisplayTime = DateTime.Now;
                var timeRunning = lastDisplayTime - timeStart;

                
                Console.WriteLine(Thread.CurrentThread.Name + " - Message/Sec: " + messagesProcessed / (timeRunning.TotalMilliseconds / 1000));
            }

            Interlocked.Increment(ref messagesProcessed);
#endif

            if (message.HeaderBytesRead < IncomingMessage.HEADER_SIZE)
            {
                if (length > 1)
                {
                    if (message.HeaderBytesRead == 0)
                    {
                        message.Header.Byte0 = buffer[offset + 0];
                        message.Header.Byte1 = buffer[offset + 1];
                        message.HeaderBytesRead += 2;

                        offset += 2;
                        length -= 2;
                    }
                    else if (message.HeaderBytesRead == 1)
                    {
                        message.Header.Byte1 = buffer[offset + 0];
                        message.HeaderBytesRead += 1;

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
                    if (message.HeaderBytesRead == 0)
                    {
                        message.Header.Byte0 = buffer[offset + 0];
                    }
                    else if (message.HeaderBytesRead == 1)
                    {
                        message.Header.Byte1 = buffer[offset + 0];
                    }
                    else
                    {
                        throw new Exception();
                    }

                    message.HeaderBytesRead += 1;
                    offset += 1;
                    length -= 1;
                }
                else
                {
                    throw new Exception();
                }

                if (message.HeaderBytesRead == 2)
                {
                    if (message.Header.Size > message.BufferSize)
                    {
                        message = new IncomingMessage();
                        message.SetBuffer(null, new byte[message.Header.Size], 0, 0, message.Header.Size);
                        message.Header.Size = (ushort)message.BufferSize;
                        message.HeaderBytesRead = 2;
                    }

                    message.Length = message.Header.Size;
                    message.DataBytesRemaining = message.Header.Size;
                }
            }

            if (length > 0)
            {
                if (length >= message.DataBytesRemaining)
                {
                    System.Buffer.BlockCopy(
                        buffer,
                        offset,
                        message.BufferHandle,
                        message.BufferOffset + message.DataBytesRead,
                        message.DataBytesRemaining
                    );

                    offset += message.DataBytesRemaining;
                    length -= message.DataBytesRemaining;

                    message.DataBytesRead += message.DataBytesRemaining;
                    message.DataBytesRemaining -= message.DataBytesRemaining;
                }
                else
                {
                    System.Buffer.BlockCopy(
                        buffer,
                        offset,
                        message.BufferHandle,
                        message.BufferOffset + message.DataBytesRead,
                        length
                    );

                    message.DataBytesRead += length;
                    message.DataBytesRemaining -= length;

                    offset += length;
                    length -= length;
                }

                message.IsDone = message.DataBytesRemaining == 0;
            }

            return message;
        }
    }
}
