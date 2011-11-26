using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public class MessageBuffer
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

            BufferId = bufferId;
            BufferSize = bufferSize;
            BufferOffset = bufferOffset;
            BufferHandle = bufferHandle;
        }

        internal void ResetBuffer()
        {
            BufferId = 0;
            BufferSize = 0;
            BufferOffset = 0;
            BufferHandle = null;
        }
    }
}
