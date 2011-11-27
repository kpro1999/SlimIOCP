using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP.Mono
{
    public class Connection : Connection<OutgoingMessage>
    {
        internal readonly Peer Peer;
        internal IncomingMessage Message;

        internal Connection(Peer peer)
        {
            Peer = peer;
        }

        public override bool TryCreateMessage(out OutgoingMessage message)
        {
            if (Peer.OutgoingMessagePool.TryPop(out message))
            {
                message.MonoConnection = this;
                return true;
            }

            message = null;
            return false;
        }

        internal virtual void Reset()
        {
            Socket = null;
            Sending = false;
            Message = null;
        }
    }
}
