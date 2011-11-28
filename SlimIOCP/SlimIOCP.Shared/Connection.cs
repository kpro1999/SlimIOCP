using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace SlimIOCP
{
    public abstract class Connection<T> where T : OutgoingMessage
    {
        public object Tag;

        public IPEndPoint RemoteEndPoint { get { return (IPEndPoint)Socket.RemoteEndPoint; } }

        internal bool Sending;
        internal bool Connected;
        internal Socket Socket;
        internal readonly Queue<T> SendQueue;

        internal Connection()
        {
            SendQueue = new Queue<T>();
        }

        public abstract bool TryCreateMessage(out T message);
    }
}
