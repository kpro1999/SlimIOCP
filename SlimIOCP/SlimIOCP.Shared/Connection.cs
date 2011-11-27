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

        public EndPoint EndPoint { get { return Socket.RemoteEndPoint; } }

        internal bool Sending;
        internal Socket Socket;
        internal readonly Queue<T> SendQueue;

        internal Connection()
        {
            SendQueue = new Queue<T>();
        }

        public abstract bool TryCreateMessage(out T message);
    }
}
