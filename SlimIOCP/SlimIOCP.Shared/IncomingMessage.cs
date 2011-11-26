using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public class IncomingMessage : MessageBuffer
    {
        public int Length { get; internal set; }
        public int Offset { get { return BufferOffset; } }
        public byte[] Data { get { return BufferHandle; } }
        public bool IsDone { get; internal set; }

        internal int DataBytesRead;
        internal int DataBytesRemaining;
        internal byte HeaderBytesRead;

        internal IncomingMessageHeader Header;
        internal readonly IncomingMessagePool Pool;

        internal IncomingMessage(IncomingMessagePool pool)
        {
            Pool = pool;
        }

        internal virtual void Reset()
        {
            Length = 0;
            IsDone = false;
            DataBytesRead = 0;
            DataBytesRemaining = 0;
            HeaderBytesRead = 0;
            Header.Size = 0;
        }
    }
}
