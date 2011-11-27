using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SlimIOCP
{
    public class IncomingMessage : MessageBuffer
    {
        public int Length { get; internal set; }
        public int Offset { get { return BufferOffset; } }
        public byte[] Data { get { return BufferHandle; } }
        public object Tag;

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
            IsDone = false;
            DataBytesRead = 0;
            DataBytesRemaining = 0;
            HeaderBytesRead = 0;
            Header.Size = 0;
            Tag = null;
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
