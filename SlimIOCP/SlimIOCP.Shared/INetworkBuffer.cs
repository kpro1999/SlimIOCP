using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public interface INetworkBuffer<TOutgoingMessage, TConnection>
        where TOutgoingMessage : OutgoingMessage
        where TConnection : Connection<TOutgoingMessage>
    {
        int BytesTransferred { get; }
        TConnection Connection { get; }
    }
}
