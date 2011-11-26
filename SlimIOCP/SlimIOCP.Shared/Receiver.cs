using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SlimIOCP
{
    /*
    internal class Receiver : BaseReceiver
    {
        readonly Peer peer;

        internal Receiver(Peer peer)
        {
            this.peer = peer;
        }

    }*/

    internal class Receiver<
            TIncomingBuffer, 
            TIncomingMessage, 
            TOutgoingMessage
        >

        where TIncomingBuffer : MessageBuffer, INetworkBuffer, IMessageBuffer<TIncomingMessage>
        where TIncomingMessage : IncomingMessage
        where TOutgoingMessage : MessageBuffer, INetworkBuffer

    {
#if DEBUG
        bool first = true;
        long messagesProcessed;
        DateTime timeStart = DateTime.Now;
        DateTime lastDisplayTime = DateTime.Now;
#endif
        readonly BasePeer<TIncomingBuffer, TIncomingMessage, TOutgoingMessage> peer;

        internal Receiver(BasePeer<TIncomingBuffer, TIncomingMessage, TOutgoingMessage> basePeer)
        {
            peer = basePeer;
        }

        internal void Start(object threadState)
        {
            Console.WriteLine("Receiver thread started");
            ReceiveLoop();
        }

        internal void ReceiveLoop()
        {
            Queue<TIncomingBuffer> queue = null;

            while (true)
            {
                lock (peer.IncomingBufferQueueSync)
                {
                    if (peer.IncomingBufferQueue.Count > 0)
                    {
                        queue = peer.IncomingBufferQueue;

                        if (!peer.IncomingBufferQueuePool.TryPop(out peer.IncomingBufferQueue))
                        {
                            //TODO: Error
                        }
                    }
                }

                if (queue != null)
                {
                    while (queue.Count > 0)
                    {
                        var buffer = queue.Dequeue();
                        var bufferHandle = buffer.BufferHandle;
                        var bufferOffset = buffer.BufferOffset;
                        var bufferLength = buffer.BytesTransferred;
                        var message = buffer.CurrentMessage;

                        while (bufferLength > 0)
                        {
                            if (message == null)
                            {
                                if (!peer.IncomingMessagePool.TryPop(out message))
                                {
                                    //TODO: Error
                                }
                            }

                            message = Receive(message, bufferHandle, ref bufferOffset, ref bufferLength);

                            if (message.IsDone)
                            {
                                message.Tag = buffer.Tag;

                                // Queue into received messages
                                lock (peer.ReceivedMessages)
                                {
                                    peer.ReceivedMessages.Enqueue(message);
                                }

                                // Signal wait event
                                peer.ReceivedMessageEvent.Set();

                                // Clear message on the buffer
                                buffer.CurrentMessage = null;
                            }
                            else
                            {
                                buffer.CurrentMessage = message;
                            }
                        }

                        if (!peer.IncomingBufferPool.TryPush(buffer))
                        {
                            //TODO: Error
                        }
                    }

                    if (!peer.IncomingBufferQueuePool.TryPush(queue))
                    {
                        //TODO: Error
                    }
                }

                peer.ReceiverEvent.Reset();

                if (peer.IncomingBufferQueue.Count == 0)
                {
                    peer.ReceiverEvent.WaitOne();
                }
            }
        }

        internal TIncomingMessage Receive(TIncomingMessage message, byte[] buffer, ref int offset, ref int length)
        {
#if DEBUG
            if (first)
            {
                timeStart = DateTime.Now;
                lastDisplayTime = timeStart;
                first = false;
            }

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
                        throw new NotImplementedException();

                        /*
                        message = new IncomingMessage();
                        message.SetBuffer(null, new byte[message.Header.Size], 0, 0, message.Header.Size);
                        message.Header.Size = (ushort)message.BufferSize;
                        message.HeaderBytesRead = 2;
                        */
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
