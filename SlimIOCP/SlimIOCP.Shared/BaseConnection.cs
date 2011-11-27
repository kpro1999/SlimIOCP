using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    public abstract class BaseConnection<T> where T : BaseOutgoingMessage
    {
        internal bool Sending;
        internal Socket Socket;
        internal readonly Queue<T> SendQueue;

        internal BaseConnection()
        {
            SendQueue = new Queue<T>();
        }

        public abstract bool TryCreateMessage(out T message);
    }
}
