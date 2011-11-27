using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP.Mono
{
    public class IncomingBuffer :
          MessageBuffer,
          INetworkBuffer<OutgoingMessage, Connection>,
          IMessageBuffer<IncomingMessage, OutgoingMessage, Connection>
    {
        internal Connection MonoConnection;

        internal override void Reset()
        {
            MonoConnection = null;
        }

        internal override void Destroy()
        {
            Reset();
        }

        internal override void BufferAssigned()
        {

        }

        public int BytesTransferred
        {
            get;
            internal set;
        }

        public Connection Connection
        {
            get { return MonoConnection; }
        }

        public IncomingMessage CurrentMessage
        {
            get
            {
                return MonoConnection.Message;
            }
            set
            {
                MonoConnection.Message = value;
            }
        }
    }
}
