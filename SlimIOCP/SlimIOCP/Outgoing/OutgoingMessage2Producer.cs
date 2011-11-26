using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    internal class OutgoingMessage2Producer : MessageBufferProducer<OutgoingMessage2>
    {
        readonly Peer peer;

        public OutgoingMessage2Producer(Peer peer)
        {
            this.peer = peer;
        }

        protected override OutgoingMessage2 Create()
        {
            var asyncArgs = new SocketAsyncEventArgs();
            var buffer = new OutgoingMessage2(peer, asyncArgs);

            asyncArgs.UserToken = buffer;
            asyncArgs.Completed += new EventHandler<SocketAsyncEventArgs>(peer.OnComplete);

            return buffer;
        }
    }
}
