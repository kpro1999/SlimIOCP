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
        internal bool Sending;
        internal Socket Socket;
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

        public bool TryCreateMessage(out OutgoingBuffer message)
        {
            SocketAsyncEventArgs asyncArgs;

            if (Peer.SendAsyncArgsPool.TryPop(out asyncArgs))
            {
                message = (OutgoingBuffer)asyncArgs.UserToken;
                message.Connection = this;
                return true;
            }

            message = null;
            return false;
        }

        internal virtual void Reset()
        {
            Socket = null;
            Sending = false;
        }
    }
}
