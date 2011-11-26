using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    internal class IncomingMessageProducer : MessageBufferProducer<IncomingMessage>
    {
        protected override IncomingMessage Create()
        {
            return new IncomingMessage();
        }
    }
}
