using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    internal class IncomingMessageProducer<TIncomingMessage, TOutgoingMessage, TConnection> : MessageBufferProducer<TIncomingMessage>
        where TOutgoingMessage : OutgoingMessage
        where TConnection : Connection<TOutgoingMessage>
        where TIncomingMessage : IncomingMessage<TOutgoingMessage, TConnection>, new()
    {
        protected override TIncomingMessage Create()
        {
            var message = new TIncomingMessage();
            message.Type = MessageType.Data;
            return message;
        }
    }
}
