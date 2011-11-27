using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP.Mono
{
    public class Peer : Peer<IncomingBuffer, IncomingMessage, OutgoingMessage, Connection>
    {
        internal readonly List<Connection> Connections;
        internal readonly ConnectionPool ConnectionPool;

        internal Peer()
        {
            IncomingBufferPool = new MessageBufferPool<IncomingBuffer>(new IncomingBufferProducer());
            IncomingMessagePool = new MessageBufferPool<IncomingMessage>(new IncomingMessageProducer<IncomingMessage, OutgoingMessage, Connection>());
            OutgoingMessagePool = new MessageBufferPool<OutgoingMessage>(new OutgoingMessageProducer(this));
            Connections = new List<Connection>();
            ConnectionPool = new ConnectionPool(this, 1024);
        }

        internal void Send(OutgoingMessage message)
        {
            if (message.MonoConnection.Sending)
            {
                //TODO: Error
            }
            else
            {
                lock (message.MonoConnection)
                {
                    message.MonoConnection.Sending = true;
                }
            }

            if (message.SendDataBuffer == null)
            {
                message.SendDataBuffer = message.BufferHandle;
            }

            message.MonoConnection.Socket.BeginSend(
                message.SendDataBuffer, 
                message.SendDataOffset, 
                message.SendDataBytesRemaining, 
                System.Net.Sockets.SocketFlags.None, 
                SendDone, 
                message
            );
        }

        internal void Receive(Connection connection)
        {
            IncomingBuffer buffer;

            if (IncomingBufferPool.TryPop(out buffer))
            {
                buffer.MonoConnection = connection;

                var result = connection.Socket.BeginReceive(
                    buffer.BufferHandle, 
                    buffer.BufferOffset, 
                    buffer.BufferSize, 
                    System.Net.Sockets.SocketFlags.None, 
                    ReceiveDone, 
                    buffer
                );

                if (result.IsCompleted)
                {

                }

                if (result.CompletedSynchronously)
                {

                }
            }
            else
            {
                //TODO: Error
            }
        }

        void SendDone(IAsyncResult result)
        {
            var message = (OutgoingMessage)result.AsyncState;
            var bytesTransferred = message.MonoConnection.Socket.EndSend(result);

            message.SendDataBytesRemaining -= bytesTransferred;
            message.SendDataBytesSent += bytesTransferred;

            if (message.SendDataBytesRemaining > 0)
            {
                Send(message);
            }
            else
            {
                var connection = message.MonoConnection;

                if (!OutgoingMessagePool.TryPush(message))
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

        void ReceiveDone(IAsyncResult result)
        {
            var buffer = (IncomingBuffer)result.AsyncState;
            var bytesTransferred = buffer.MonoConnection.Socket.EndReceive(result);
            var connection = buffer.MonoConnection;

            if (bytesTransferred == 0)
            {
                IncomingBufferPool.TryPush(buffer);
                return;
            }

            buffer.BytesTransferred = bytesTransferred;

            lock (IncomingBufferQueueSync)
            {
                IncomingBufferQueue.Enqueue(buffer);
            }

            ReceiverEvent.Set();
            Receive(connection);
        }
    }
}
