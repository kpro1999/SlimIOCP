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
        internal readonly Queue<SocketAsyncEventArgs> SendQueue;
        internal readonly Queue<IncomingMessage> ReceiveQueue;

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

        public DataToken CreateMessage()
        {
            SocketAsyncEventArgs asyncArgs;

            Peer.SendAsyncArgsPool.TryPop(out asyncArgs);

            var token = (DataToken)asyncArgs.UserToken;

            token.Connection = this;

            return token;
        }
    }
}
