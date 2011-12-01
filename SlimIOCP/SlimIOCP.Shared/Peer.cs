using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace SlimIOCP
{
    public abstract class Peer<
            TIncomingBuffer,
            TIncomingMessage,
            TOutgoingMessage,
            TConnection
        >

        where TIncomingBuffer : MessageBuffer, INetworkBuffer<TOutgoingMessage, TConnection>, 
                                IMessageBuffer<TIncomingMessage, TOutgoingMessage, TConnection>
        where TIncomingMessage : IncomingMessage<TOutgoingMessage, TConnection>, new()
        where TOutgoingMessage : OutgoingMessage, INetworkBuffer<TOutgoingMessage, TConnection>
        where TConnection : Connection<TOutgoingMessage>
    {
        static internal int ReceiverThreadIdCounter = -1;

        internal MessageBufferPool<TIncomingBuffer> IncomingBufferPool;
        internal MessageBufferPool<TIncomingMessage> IncomingMessagePool;
        internal MessageBufferPool<TOutgoingMessage> OutgoingMessagePool;
        internal readonly MetaMessagePool<TIncomingMessage, TOutgoingMessage, TConnection> MetaMessagePool;

        internal Queue<TIncomingBuffer> IncomingBufferQueue;
        internal readonly object IncomingBufferQueueSync = new object();
        internal readonly QueuePool<TIncomingBuffer> IncomingBufferQueuePool;
        internal readonly Queue<TIncomingMessage> ReceivedMessages;

        internal Socket Socket;
        internal Thread ReceiverThread;

        internal readonly Receiver<TIncomingBuffer, TIncomingMessage, TOutgoingMessage, TConnection> Receiver;
        internal readonly ManualResetEvent ReceiverEvent;

        public readonly AutoResetEvent ReceivedMessageEvent;

        public IPEndPoint EndPoint { get; private set; }

        public Peer()
        {
            ReceiverEvent = new ManualResetEvent(true);
            ReceivedMessageEvent = new AutoResetEvent(false);
            ReceivedMessages = new Queue<TIncomingMessage>();
            Receiver = new Receiver<TIncomingBuffer, TIncomingMessage, TOutgoingMessage, TConnection>(this);
            ReceiverThread = new Thread(Receiver.Start);

            IncomingBufferQueue = new Queue<TIncomingBuffer>();
            IncomingBufferQueuePool = new QueuePool<TIncomingBuffer>(32);

            MetaMessagePool = new MetaMessagePool<TIncomingMessage, TOutgoingMessage, TConnection>(256);
        }

        public bool TryRecycleMessage(TIncomingMessage message)
        {
            if (message.Type == MessageType.Data)
            {
                lock (IncomingMessagePool)
                {
                    return IncomingMessagePool.TryPush(message);
                }
            }
            else
            {
                MetaMessagePool.Push(message);
                return true;
            }
        }

        public bool TryPopMessage(out TIncomingMessage message)
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

        internal void PushMessage(TIncomingMessage message)
        {
            lock (ReceivedMessages)
            {
                ReceivedMessages.Enqueue(message);
            }

            ReceivedMessageEvent.Set();
        }

        internal void StartReceiver()
        {
            if (!ReceiverThread.IsAlive)
            {
                ReceiverThread = new Thread(Receiver.Start);
                ReceiverThread.IsBackground = true;
                ReceiverThread.Name = "SlimIOCP Receiver Thread #" + Interlocked.Increment(ref ReceiverThreadIdCounter);
                ReceiverThread.Start();
            }
            else
            {
                SlimCommon.Log.Default.Info("Thread already running");
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

        protected void ShutdownSocket(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {

            }
        }

    }
}
