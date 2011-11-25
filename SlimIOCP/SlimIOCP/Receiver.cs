using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    internal class Receiver
    {
        Queue<SocketAsyncEventArgs> receiveQueue;

        readonly Peer peer;

        internal Receiver(Peer peer)
        {
            this.peer = peer;
        }

        internal void Start(object threadState)
        {
            Console.WriteLine("Receiver thread started");
            Receive();
        }

        void Receive()
        {
            while (true)
            {
                lock (peer.ReceiveQueueSync)
                {
                    if (peer.ReceiveQueue.Count > 0)
                    {
                        receiveQueue = peer.ReceiveQueue;
                        peer.ReceiveQueue = new Queue<SocketAsyncEventArgs>();
                    }
                }

                while (receiveQueue != null && receiveQueue.Count > 0)
                {
                    var asyncArgs = receiveQueue.Dequeue();
                    var token = (DataToken)asyncArgs.UserToken;
                    var connection = token.Connection;

                    var buffer = asyncArgs.Buffer;
                    var bufferOffset = token.BufferOffset;
                    var bufferLength = asyncArgs.BytesTransferred;

                    while (bufferLength > 0)
                    {
                        if (connection.Message == null)
                        {
                            if (!peer.IncomingMessagePool.TryPop(out connection.Message))
                            {
                                //TODO: Error
                            }
                        }

                        connection.Message = ReceiverUtils.Receive(connection.Message, buffer, ref bufferOffset, ref bufferLength);

                        if (connection.Message.IsDone)
                        {
                            lock (connection.ReceiveQueue)
                            {
                                connection.ReceiveQueue.Enqueue(connection.Message);
                            }

                            connection.Message = null;
                        }
                    }

                    if (!peer.ReceiveAsyncArgsPool.TryPush(asyncArgs))
                    {
                        //TODO: Error
                    }
                }

                peer.ReceiverEvent.Reset();

                if (peer.ReceiveQueue.Count == 0)
                {
                    peer.ReceiverEvent.WaitOne();
                }
            }
        }
    }
}
