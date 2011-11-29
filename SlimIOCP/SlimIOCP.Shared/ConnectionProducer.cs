using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    internal abstract class ConnectionProducer<TOutgoingMessage, TConnection>
        where TOutgoingMessage : OutgoingMessage
        where TConnection : Connection<TOutgoingMessage>
    {
        internal abstract TConnection Create();
    }
}
