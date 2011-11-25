using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace SlimIOCP
{
    public class Connection
    {
        internal bool Pooled;
        internal bool Sending;
        internal Socket Socket;
        internal bool IsQueued;

        internal IncomingMessage Message;

        internal readonly Peer Peer;
        internal readonly Queue<IncomingMessage> ReceiveQueue;
        internal readonly Queue<SocketAsyncEventArgs> SendQueue;

        internal Connection(Peer peer)
        {
            Peer = peer;
            SendQueue = new Queue<SocketAsyncEventArgs>();
            ReceiveQueue = new Queue<IncomingMessage>();
        }

        internal void MessageComplete()
        {
            ReceiveQueue.Enqueue(Message);
        }

        public bool TryCreateMessage(out DataToken message)
        {
            SocketAsyncEventArgs asyncArgs;

            if (Peer.SendAsyncArgsPool.TryPop(out asyncArgs))
            {
                message = (DataToken)asyncArgs.UserToken;
                message.Connection = this;
                return true;
            }

            message = null;
            return false;
        }
    }
}
