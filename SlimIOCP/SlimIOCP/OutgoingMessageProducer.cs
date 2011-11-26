using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    internal class OutgoingMessageProducer : MessageBufferProducer<OutgoingMessage>
    {
        readonly Peer peer;

        public OutgoingMessageProducer(Peer peer)
        {
            this.peer = peer;
        }

        protected override OutgoingMessage Create()
        {
            var asyncArgs = new SocketAsyncEventArgs();
            var buffer = new OutgoingMessage(peer, asyncArgs);

            asyncArgs.UserToken = buffer;
            asyncArgs.Completed += new EventHandler<SocketAsyncEventArgs>(peer.OnComplete);

            return buffer;
        }
    }
}
