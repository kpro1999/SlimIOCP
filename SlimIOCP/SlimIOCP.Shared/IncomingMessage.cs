using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public class IncomingMessage
    {
        public const int HeaderSize = 2;

        public int Length { get; internal set; }
        public int Offset { get { return BufferOffset; } }
        public byte[] Data { get { return BufferHandle; } }
        public bool IsDone { get; internal set; }

        internal int BytesRead;
        internal int BytesRemaining;
        internal byte HeaderBytesReceived;
        internal ShortConverter Header;

        internal readonly int BufferId;
        internal readonly int BufferSize;
        internal readonly int BufferOffset;
        internal readonly byte[] BufferHandle;
        internal readonly IncomingMessagePool Pool;

        internal IncomingMessage(IncomingMessagePool pool, int bufferId, int bufferOffset, int bufferSize, byte[] bufferHandle)
        {
            Pool = pool;
            BufferSize = bufferSize;
            BufferHandle = bufferHandle;
            BufferOffset = bufferOffset;
            BufferId = bufferId;
        }
    }
}
