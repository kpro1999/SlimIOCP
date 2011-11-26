using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP.Mono
{
    public class Peer : BasePeer
    {
        public override bool TryGetMessage(out IncomingMessage message)
        {
            throw new NotImplementedException();
        }

        public override bool TryRecycleMessage(IncomingMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
