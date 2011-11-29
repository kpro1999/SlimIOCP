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

        internal bool Sending;
        internal bool Connected;

        internal Socket Socket;
        internal readonly Queue<T> SendQueue;

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                if (Socket != null)
                    return (IPEndPoint)Socket.RemoteEndPoint;

                return null;
            }
        }

        public abstract bool TryCreateMessage(out T message);

        internal Connection()
        {
            SendQueue = new Queue<T>();
        }

        internal virtual void Reset()
        {
            Sending = false;
            Connected = false;
            Socket = null;
            SendQueue.Clear();
            Tag = null;
        }
    }
}
