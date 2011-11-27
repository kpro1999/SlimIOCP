using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SlimIOCP
{
    public abstract class IncomingMessage<TOutgoingMessage, TConnection> : MessageBuffer
        where TOutgoingMessage : OutgoingMessage
        where TConnection : Connection<TOutgoingMessage>
    {
        public int Length { get; internal set; }
        public int Offset { get { return BufferOffset; } }
        public byte[] Buffer { get { return BufferHandle; } }
        public TConnection Connection { get; internal set; }
        public MessageType MessageType { get { return Type; } }

        internal MessageType Type;
        internal bool IsDone;
        internal int DataBytesRead;
        internal int DataBytesRemaining;
        internal byte HeaderBytesRead;
        internal IncomingMessageHeader Header;

        internal IncomingMessage()
        {

        }

        internal override void Reset()
        {
            Length = 0;
            Connection = null;
            IsDone = false;
            DataBytesRead = 0;
            DataBytesRemaining = 0;
            HeaderBytesRead = 0;
            Header.Size = 0;
        }

        internal override void Destroy()
        {
            Reset();
        }

        internal override void BufferAssigned()
        {

        }
    }
}
