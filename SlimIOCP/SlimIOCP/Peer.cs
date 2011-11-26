using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SlimIOCP
{
    public class Peer
    {
        static int ThreadId = 0;

        internal Socket Socket;
        internal Queue<SocketAsyncEventArgs> IncomingBufferQueue;
        internal readonly object IncomingBufferQueueSync = new object();

        internal readonly Receiver Receiver;
        internal readonly Thread ReceiverThread;
        internal readonly ManualResetEvent ReceiverEvent;
        internal readonly OutgoingBufferAsyncArgsPool SendAsyncArgsPool;
        internal readonly IncomingBufferAsyncArgsPool ReceiveAsyncArgsPool;
        internal readonly IncomingMessagePool IncomingMessagePool;
        internal readonly QueuePool<SocketAsyncEventArgs> AsyncArgsQueuePool;

        public IPEndPoint EndPoint { get; private set; }

        internal Peer()
        {
            Receiver = new Receiver(this);
            ReceiverEvent = new ManualResetEvent(true);
            ReceiverThread = new Thread(Receiver.Start);
            IncomingBufferQueue = new Queue<SocketAsyncEventArgs>();
            SendAsyncArgsPool = new OutgoingBufferAsyncArgsPool(this);
            ReceiveAsyncArgsPool = new IncomingBufferAsyncArgsPool(this);
            IncomingMessagePool = new IncomingMessagePool(1024);
            AsyncArgsQueuePool = new QueuePool<SocketAsyncEventArgs>(32);
        }

        internal void InitSocket(IPEndPoint endPoint)
        {
            if (Socket == null)
            {
                EndPoint = endPoint;
                Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                ReceiverThread.IsBackground = true;
                ReceiverThread.Name = "SlimIOCP Receiver Thread #" + Interlocked.Increment(ref ThreadId);
                ReceiverThread.Start();
            }
            else
            {
                //TODO: Error
            }
        }

        internal void OnComplete(object sender, SocketAsyncEventArgs asyncArgs)
        {
            switch (asyncArgs.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    OnSendComplete(asyncArgs);
                    break;

                case SocketAsyncOperation.Receive:
                    OnReceiveComplete(asyncArgs);
                    break;

                default:
                    //TODO: Error
                    throw new Exception("Not send or receive");
            }
        }

        public void ReceiveAsync(Connection connection)
        {
            SocketAsyncEventArgs asyncArgs;

            if (ReceiveAsyncArgsPool.TryPop(out asyncArgs))
            {
                var buffer = (IncomingBuffer)asyncArgs.UserToken;
                buffer.Connection = connection;

                var isDone = !connection.Socket.ReceiveAsync(asyncArgs);
                if (isDone)
                {
                    OnReceiveComplete(asyncArgs);
                }
            }
            else
            {
                //TODO: Error
            }
        }

        internal void SendAsync(SocketAsyncEventArgs asyncArgs)
        {
            var buffer = (OutgoingBuffer)asyncArgs.UserToken;

            if (!buffer.Connection.Sending)
            {
                lock (buffer.Connection)
                {
                    buffer.Connection.Sending = true;
                }
            }

            // Common case
            if (buffer.SendDataBuffer == null)
            {
                if (buffer.SendDataBytesSent != 0)
                {
                    asyncArgs.SetBuffer(buffer.BufferOffset + buffer.SendDataBytesSent, buffer.BufferSize - buffer.SendDataBytesSent);
                }
            }
            else
            {
                var dataOffset = buffer.SendDataOffset + buffer.SendDataBytesSent;
                var sendLength = Math.Min(buffer.SendDataBytesRemaining, buffer.BufferSize);
                asyncArgs.SetBuffer(buffer.BufferOffset, sendLength);
                System.Buffer.BlockCopy(buffer.SendDataBuffer, dataOffset, asyncArgs.Buffer, buffer.BufferOffset, sendLength);
            }

            var isDone = !buffer.Connection.Socket.SendAsync(asyncArgs);
            if (isDone)
            {
                OnSendComplete(asyncArgs);
            }
        }

        void OnReceiveComplete(SocketAsyncEventArgs asyncArgs)
        {
            if (asyncArgs.BytesTransferred == 0)
            {
                ReceiveAsyncArgsPool.TryPush(asyncArgs);
                return;
            }

            var connection = ((IncomingBuffer)asyncArgs.UserToken).Connection;

            lock (IncomingBufferQueueSync)
            {
                IncomingBufferQueue.Enqueue(asyncArgs);
            }

            ReceiverEvent.Set();
            ReceiveAsync(connection);
        }

        void OnSendComplete(SocketAsyncEventArgs asyncArgs)
        {
            if (asyncArgs.SocketError != SocketError.Success)
            {
                return;
            }

            var buffer = (OutgoingBuffer)asyncArgs.UserToken;
            buffer.SendDataBytesRemaining -= asyncArgs.BytesTransferred;
            buffer.SendDataBytesSent += asyncArgs.BytesTransferred;

            if (buffer.SendDataBytesRemaining > 0)
            {
                SendAsync(asyncArgs);
            }
            else
            {
                var connection = buffer.Connection;

                if (!SendAsyncArgsPool.TryPush(asyncArgs))
                {
                    //TODO: Error
                }

                lock (connection)
                {
                    if (connection.SendQueue.Count > 0)
                    {
                        SendAsync(connection.SendQueue.Dequeue());
                    }
                    else
                    {
                        connection.Sending = false;
                    }
                }
            }
        }
    }
}
