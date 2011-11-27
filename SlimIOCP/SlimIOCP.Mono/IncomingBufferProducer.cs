using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP.Mono
{
    internal class IncomingBufferProducer : MessageBufferProducer<IncomingBuffer>
    {
        protected override IncomingBuffer Create()
        {
            return new IncomingBuffer();
        }
    }
}
