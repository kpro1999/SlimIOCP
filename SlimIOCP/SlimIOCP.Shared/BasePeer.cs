using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace SlimIOCP
{
    public abstract class BasePeer
    {
        static internal int ReceiverThreadIdCounter = -1;

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
        }

        public abstract bool TryGetMessage(out IncomingMessage message);
        public abstract bool TryRecycleMessage(IncomingMessage message);

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
