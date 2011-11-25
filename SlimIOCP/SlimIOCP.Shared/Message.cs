using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public abstract class Message
    {
        public const int HEADER_SIZE = 2;

        internal int BufferId;
        internal int BufferSize;
        internal int BufferOffset;
        internal byte[] BufferHandle;

        internal void SetBuffer(byte[] bufferHandle, int bufferId, int bufferOffset, int bufferSize)
        {
#if DEBUG
            if (bufferHandle == null)
            {
                throw new ArgumentNullException("bufferHandle");
            }
#endif

            BufferHandle = bufferHandle;
            BufferId = bufferId;
            BufferSize = bufferSize;
            BufferOffset = bufferOffset;
        }
    }
}
