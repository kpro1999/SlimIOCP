using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP.Mono
{
    internal class OutgoingMessageProducer : MessageBufferProducer<OutgoingMessage>
    {
        Peer peer;

        internal OutgoingMessageProducer(Peer monoPeer)
        {
            peer = monoPeer;
        }

        protected override OutgoingMessage Create()
        {
            return new OutgoingMessage(peer);
        }
    }
}
