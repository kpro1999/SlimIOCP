using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace SlimIOCP
{
    public abstract class BasePeer<
            TIncomingBuffer,
            TIncomingMessage,
            TOutgoingMessage,
            TConnection
        >

        where TIncomingBuffer : MessageBuffer, INetworkBuffer<TOutgoingMessage, TConnection>, 
                                IMessageBuffer<TIncomingMessage, TOutgoingMessage, TConnection>
        where TIncomingMessage : IncomingMessage<TOutgoingMessage, TConnection>
        where TOutgoingMessage : BaseOutgoingMessage, INetworkBuffer<TOutgoingMessage, TConnection>
        where TConnection : BaseConnection<TOutgoingMessage>
    {
        static internal int ReceiverThreadIdCounter = -1;

        internal MessageBufferPool<TIncomingBuffer> IncomingBufferPool;
        internal MessageBufferPool<TIncomingMessage> IncomingMessagePool;
        internal MessageBufferPool<TOutgoingMessage> OutgoingMessagePool;

        internal Queue<TIncomingBuffer> IncomingBufferQueue;
        internal readonly object IncomingBufferQueueSync = new object();
        internal readonly QueuePool<TIncomingBuffer> IncomingBufferQueuePool;
        internal readonly Queue<TIncomingMessage> ReceivedMessages;

        internal Socket Socket;
        internal readonly Receiver<TIncomingBuffer, TIncomingMessage, TOutgoingMessage, TConnection> Receiver;
        internal readonly Thread ReceiverThread;
        internal readonly ManualResetEvent ReceiverEvent;
        public readonly ManualResetEvent ReceivedMessageEvent;

        public IPEndPoint EndPoint { get; private set; }

        public BasePeer()
        {
            ReceiverEvent = new ManualResetEvent(true);
            ReceivedMessageEvent = new ManualResetEvent(false);
            ReceivedMessages = new Queue<TIncomingMessage>();
            Receiver = new Receiver<TIncomingBuffer, TIncomingMessage, TOutgoingMessage, TConnection>(this);
            ReceiverThread = new Thread(Receiver.Start);

            IncomingBufferQueue = new Queue<TIncomingBuffer>();
            IncomingBufferQueuePool = new QueuePool<TIncomingBuffer>(32);
        }

        public bool TryRecycleMessage(TIncomingMessage message)
        {
            lock (IncomingMessagePool)
            {
                return IncomingMessagePool.TryPush(message);
            }
        }

        public bool TryGetMessage(out TIncomingMessage message)
        {
            lock (ReceivedMessages)
            {
                if (ReceivedMessages.Count > 0)
                {
                    message = ReceivedMessages.Dequeue();
                    return true;
                }
            }

            ReceivedMessageEvent.Reset();

            message = null;
            return false;
        }

        internal void StartReceiver()
        {
            if (!ReceiverThread.IsAlive)
            {
                ReceiverThread.IsBackground = true;
                ReceiverThread.Name = "SlimIOCP Receiver Thread #" + Interlocked.Increment(ref ReceiverThreadIdCounter);
                ReceiverThread.Start();
            }
        }

        internal void InitSocket(IPEndPoint endPoint)
        {
            if (Socket == null)
            {
                EndPoint = endPoint;

                Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

                StartReceiver();
            }
            else
            {
                //TODO: Error
            }
        }

    }
}
