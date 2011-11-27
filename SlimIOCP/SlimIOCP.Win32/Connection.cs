using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace SlimIOCP.Win32
{
    public class Connection : BaseConnection<OutgoingMessage>
    {
        internal IncomingMessage Message;
        internal readonly Peer Peer;

        internal Connection(Peer peer)
        {
            Peer = peer;
        }

        public override bool TryCreateMessage(out OutgoingMessage message)
        {
            if (Peer.OutgoingMessagePool.TryPop(out message))
            {
                message.Win32Connection = this;
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
