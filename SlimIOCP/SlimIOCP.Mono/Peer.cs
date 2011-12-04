using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SlimCommon;

namespace SlimIOCP.Mono
{
    public abstract class Peer : Peer<IncomingBuffer, IncomingMessage, OutgoingMessage, Connection>
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
            var connection = message.MonoConnection;
            if (connection.Sending)
            {
                //TODO: Error
            }
            else
            {
                lock (connection)
                {
                    connection.Sending = true;
                }
            }

            if (message.SendDataBuffer == null)
            {
                message.SendDataBuffer = message.BufferHandle;
            }

            try
            {
                connection.Socket.BeginSend(
                    message.SendDataBuffer,
                    message.SendDataOffset,
                    message.SendDataBytesRemaining,
                    System.Net.Sockets.SocketFlags.None,
                    SendDone,
                    message
                );
            }
            catch (SocketException)
            {
                Disconnect(connection);

                if (!OutgoingMessagePool.TryPush(message))
                {
                    //TODO: Error
                }
            }
            catch (NullReferenceException)
            {
                // This means that socket was null (connection was already closed)
                // We can just ignore this
            }
        }

        internal void Receive(Connection connection)
        {
#if DEBUG
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

#endif
            IncomingBuffer buffer;

            if (IncomingBufferPool.TryPop(out buffer))
            {
                buffer.MonoConnection = connection;

                try
                {
                    connection.Socket.BeginReceive(
                        buffer.BufferHandle,
                        buffer.BufferOffset,
                        buffer.BufferSize,
                        System.Net.Sockets.SocketFlags.None,
                        ReceiveDone,
                        buffer
                    );
                }
                catch (SocketException)
                {
                    Disconnect(connection);
                    IncomingBufferPool.TryPush(buffer);
                }
                catch (NullReferenceException)
                {
                    // This means that socket was null (connection was already closed)
                    // We can just ignore this
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
            var connection = message.MonoConnection;

            try
            {
                var bytesTransferred = connection.Socket.EndSend(result);

                message.SendDataBytesRemaining -= bytesTransferred;
                message.SendDataBytesSent += bytesTransferred;

                if (message.SendDataBytesRemaining > 0)
                {
                    Send(message);
                }
                else
                {
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
            catch (SocketException)
            {
                Disconnect(connection);

                if (!OutgoingMessagePool.TryPush(message))
                {
                    //TODO: Error
                }
            }
        }

        void ReceiveDone(IAsyncResult result)
        {
            var buffer = (IncomingBuffer)result.AsyncState;
            var connection = buffer.MonoConnection;

            try
            {
                var bytesTransferred = connection.Socket.EndReceive(result);

#if DEBUG
                if (bytesTransferred == 0)
                {
                    Disconnect(connection);

                    if (!IncomingBufferPool.TryPush(buffer))
                    {
                        //TODO: Error
                    }

                    return;
                }
#endif

                buffer.BytesTransferred = bytesTransferred;

                lock (IncomingBufferQueueSync)
                {
                    IncomingBufferQueue.Enqueue(buffer);
                }

                ReceiverEvent.Set();
                Receive(connection);
            }
            catch (SocketException)
            {
                Disconnect(connection);

                if (!IncomingBufferPool.TryPush(buffer))
                {
                    //TODO: Error
                }
            }
        }

        public void Disconnect(Connection connection)
        {
            SlimCommon.Log.Default.Info("[SlimIOCP] Disconnected " + connection.RemoteEndPoint);

            lock (connection)
            {
                if (connection.Connected)
                {
                    connection.Connected = false;
                    Connections.Remove(connection);
                    PushMessage(MetaMessagePool.Pop(MessageType.Disconnected, connection));
                    ShutdownSocket(connection.Socket);
                }
            }
        }

        public void RecycleConnection(Connection connection)
        {
#if DEBUG
            if (connection.Connected == true)
            {
                //TODO: Error
                throw new Exception();
            }
#endif

            Log.Default.Info("[SlimIOCP] Recycling connection");

            if (!ConnectionPool.TryPush(connection))
            {
                //TODO: Error
            }
        }
    }
}
