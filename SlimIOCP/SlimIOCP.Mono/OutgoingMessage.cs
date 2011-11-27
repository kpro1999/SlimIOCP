using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP.Mono
{
    public class OutgoingMessage : SlimIOCP.OutgoingMessage, INetworkBuffer<OutgoingMessage, Connection>
    {
        public int BytesTransferred
        {
            get { throw new NotImplementedException(); }
        }

        public Connection Connection
        {
            get { throw new NotImplementedException(); }
        }

        public override bool TryQueue()
        {
            throw new NotImplementedException();
        }

        internal override void BufferAssigned()
        {
            throw new NotImplementedException();
        }
    }
}
