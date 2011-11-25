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
        internal Socket Socket;
        internal Queue<SocketAsyncEventArgs> ReceiveQueue;
        internal readonly object ReceiveQueueSync = new object();

        internal readonly Receiver Receiver;
        internal readonly Thread ReceiverThread;
        internal readonly ManualResetEvent ReceiverEvent;
        internal readonly DataAsyncArgsPool SendAsyncArgsPool;
        internal readonly DataAsyncArgsPool ReceiveAsyncArgsPool;
        internal readonly IncomingMessagePool IncomingMessagePool;

        public IPEndPoint EndPoint { get; private set; }

        internal Peer()
        {
            Receiver = new Receiver(this);
            ReceiverEvent = new ManualResetEvent(true);
            ReceiverThread = new Thread(Receiver.Start);
            ReceiveQueue = new Queue<SocketAsyncEventArgs>();
            SendAsyncArgsPool = new DataAsyncArgsPool(this);
            ReceiveAsyncArgsPool = new DataAsyncArgsPool(this);
            IncomingMessagePool = new IncomingMessagePool(1024);
        }

        internal void InitSocket(IPEndPoint endPoint)
        {
            if (Socket == null)
            {
                EndPoint = endPoint;
                Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
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
                var dataToken = (DataToken)asyncArgs.UserToken;
                dataToken.Connection = connection;

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
            var token = (DataToken)asyncArgs.UserToken;

            if (!token.Connection.Sending)
            {
                lock (token.Connection)
                {
                    token.Connection.Sending = true;
                }
            }

            // Common case
            if (token.SendData == null)
            {
                if (token.SendDataBytesSent != 0)
                {
                    asyncArgs.SetBuffer(token.BufferOffset + token.SendDataBytesSent, token.BufferSize - token.SendDataBytesSent);
                }
            }
            else
            {
                var dataOffset = token.SendDataOffset + token.SendDataBytesSent;
                var sendLength = Math.Min(token.SendDataBytesRemaining, token.BufferSize);
                asyncArgs.SetBuffer(token.BufferOffset, sendLength);
                System.Buffer.BlockCopy(token.SendData, dataOffset, asyncArgs.Buffer, token.BufferOffset, sendLength);
            }

            var isDone = !token.Connection.Socket.SendAsync(asyncArgs);
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

            var connection = ((DataToken)asyncArgs.UserToken).Connection;

            lock (ReceiveQueueSync)
            {
                ReceiveQueue.Enqueue(asyncArgs);
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

            var token = (DataToken)asyncArgs.UserToken;
            token.SendDataBytesRemaining -= asyncArgs.BytesTransferred;
            token.SendDataBytesSent += asyncArgs.BytesTransferred;

            if (token.SendDataBytesRemaining > 0)
            {
                SendAsync(asyncArgs);
            }
            else
            {
                var connection = token.Connection;

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
