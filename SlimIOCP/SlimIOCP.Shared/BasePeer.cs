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
            TOutgoingMessage
        
            /*, 
            TIncomingBufferProducer,
            TIncomingMessageProducer,
            TOutgoingMessageProducer
            */
        >

        where TIncomingBuffer : MessageBuffer
        where TIncomingMessage : MessageBuffer
        where TOutgoingMessage : MessageBuffer

        /*
        where TIncomingBufferProducer : MessageBufferProducer<TIncomingBuffer>, new()
        where TIncomingMessageProducer : MessageBufferProducer<TIncomingMessage>, new()
        where TOutgoingMessageProducer : MessageBufferProducer<TOutgoingMessage>, new()
        */
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
        internal Thread ReceiverThread;
        internal BaseReceiver Receiver;

        internal readonly ManualResetEvent ReceiverEvent;

        public readonly ManualResetEvent ReceivedMessageEvent;
        public IPEndPoint EndPoint { get; private set; }

        public BasePeer()
        {
            ReceiverEvent = new ManualResetEvent(true);
            ReceivedMessageEvent = new ManualResetEvent(false);
            ReceivedMessages = new Queue<TIncomingMessage>();

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
            if (ReceiverThread == null)
            {
                ReceiverThread = new Thread(Receiver.Start);
                ReceiverThread.IsBackground = true;
                ReceiverThread.Name = "SlimIOCP Receiver Thread #" + Interlocked.Increment(ref ReceiverThreadIdCounter);
                ReceiverThread.Start();
            }
            else
            {
                //TODO: Error
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
