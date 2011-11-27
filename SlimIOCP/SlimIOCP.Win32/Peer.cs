using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SlimIOCP.Win32
{
    public class Peer : BasePeer<IncomingBuffer, IncomingMessage, OutgoingMessage>
    {
        internal readonly List<Connection> Connections;
        internal readonly ConnectionPool ConnectionPool;

        internal Peer()
            : base()
        {
            IncomingBufferPool = new MessageBufferPool<IncomingBuffer>(new IncomingBufferProducer(this));
            IncomingMessagePool = new MessageBufferPool<IncomingMessage>(new IncomingMessageProducer());
            OutgoingMessagePool = new MessageBufferPool<OutgoingMessage>(new OutgoingMessageProducer(this));
            Connections = new List<Connection>();
            ConnectionPool = new ConnectionPool(this, 1024);
        }

        internal void OnComplete(object sender, SocketAsyncEventArgs asyncArgs)
        {
            switch (asyncArgs.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    OnSendComplete((OutgoingMessage)asyncArgs.UserToken);
                    break;

                case SocketAsyncOperation.Receive:
                    OnReceiveComplete((IncomingBuffer)asyncArgs.UserToken);
                    break;

                default:
                    //TODO: Error
                    throw new Exception("Not send or receive");
            }
        }

        internal void Receive(Connection connection)
        {
            IncomingBuffer buffer;

            if (IncomingBufferPool.TryPop(out buffer))
            {
                buffer.Connection = connection;

                var isDone = !connection.Socket.ReceiveAsync(buffer.AsyncArgs);
                if (isDone)
                {
                    OnReceiveComplete(buffer);
                }
            }
            else
            {
                //TODO: Error
            }
        }

        internal void Send(OutgoingMessage message)
        {
            if (message.Connection.Sending)
            {
                //TODO: Error
            }
            else
            {
                lock (message.Connection)
                {
                    message.Connection.Sending = true;
                }
            }

            // Common case
            if (message.SendDataBuffer == null)
            {
                message.AsyncArgs.SetBuffer(message.BufferOffset + message.SendDataBytesSent, message.SendDataBytesRemaining);
            }
            else
            {
                var dataOffset = message.SendDataOffset + message.SendDataBytesSent;
                var sendLength = Math.Min(message.SendDataBytesRemaining, message.BufferSize);
                message.AsyncArgs.SetBuffer(message.BufferOffset, sendLength);
                System.Buffer.BlockCopy(message.SendDataBuffer, dataOffset, message.BufferHandle, message.BufferOffset, sendLength);
            }

            var isDone = !message.Connection.Socket.SendAsync(message.AsyncArgs);
            if (isDone)
            {
                OnSendComplete(message);
            }
        }

        void OnReceiveComplete(IncomingBuffer buffer)
        {
            if (buffer.AsyncArgs.BytesTransferred == 0)
            {
                IncomingBufferPool.TryPush(buffer);
                return;
            }

            var connection = buffer.Connection;

            lock (IncomingBufferQueueSync)
            {
                IncomingBufferQueue.Enqueue(buffer);
            }

            ReceiverEvent.Set();
            Receive(connection);
        }

        void OnSendComplete(OutgoingMessage message)
        {
            if (message.AsyncArgs.SocketError != SocketError.Success)
            {
                return;
            }

            message.SendDataBytesRemaining -= message.AsyncArgs.BytesTransferred;
            message.SendDataBytesSent += message.AsyncArgs.BytesTransferred;

            if (message.SendDataBytesRemaining > 0)
            {
                Send(message);
            }
            else
            {
                var connection = message.Connection;

                if (OutgoingMessagePool.TryPush(message))
                {
                    //TODO: Error
                }

                lock (connection)
                {
                    if (connection.SendQueue.Count > 0)
                    {
                        Send(connection.SendQueue.Dequeue());
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
