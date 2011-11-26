using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public interface IMessageBuffer<TIncomingMessage> 
        where TIncomingMessage : IncomingMessage
    {
        TIncomingMessage CurrentMessage { get; set; }
    }
}
