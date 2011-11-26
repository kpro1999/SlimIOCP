using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP.Mono
{
    public class IncomingBuffer : MessageBuffer, INetworkBuffer, IMessageBuffer<IncomingMessage>
    {
        public int BytesTransferred
        {
            get { throw new NotImplementedException(); }
        }

        public IncomingMessage CurrentMessage
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        internal override void Reset()
        {
            throw new NotImplementedException();
        }

        internal override void Destroy()
        {
            throw new NotImplementedException();
        }

        internal override void BufferAssigned()
        {
            throw new NotImplementedException();
        }
    }
}
