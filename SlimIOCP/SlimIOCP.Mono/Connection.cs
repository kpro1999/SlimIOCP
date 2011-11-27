using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP.Mono
{
    public class Connection : BaseConnection<OutgoingMessage>
    {
        public override bool TryCreateMessage(out OutgoingMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
