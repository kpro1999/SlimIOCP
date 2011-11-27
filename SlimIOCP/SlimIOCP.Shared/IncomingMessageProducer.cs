using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    internal class IncomingMessageProducer<TIncomingMessage, TOutgoingMessage, TConnection> : MessageBufferProducer<TIncomingMessage>
        where TOutgoingMessage : BaseOutgoingMessage
        where TConnection : BaseConnection<TOutgoingMessage>
        where TIncomingMessage : IncomingMessage<TOutgoingMessage, TConnection>, new()
    {
        protected override TIncomingMessage Create()
        {
            return new TIncomingMessage();
        }
    }
}
