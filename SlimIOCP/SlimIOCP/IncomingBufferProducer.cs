using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    internal class IncomingBufferProducer : MessageBufferProducer<IncomingBuffer>
    {
        readonly Peer peer;

        public IncomingBufferProducer(Peer peer)
        {
            this.peer = peer;
        }

        protected override IncomingBuffer Create()
        {
            var asyncArgs = new SocketAsyncEventArgs();
            var buffer = new IncomingBuffer(peer, asyncArgs);

            asyncArgs.UserToken = buffer;
            asyncArgs.Completed += new EventHandler<SocketAsyncEventArgs>(peer.OnComplete);

            return buffer;
        }
    }
}
