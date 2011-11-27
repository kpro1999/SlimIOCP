using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public abstract class MessageBuffer
    {
        internal int BufferId;
        internal int BufferSize;
        internal int BufferOffset;
        internal byte[] BufferHandle;
        internal BufferManager BufferManager;

        internal void SetBuffer(BufferManager bufferManager, byte[] bufferHandle, int bufferId, int bufferOffset, int bufferSize)
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
            BufferManager = bufferManager;
        }

        internal void ClearBuffer()
        {
            BufferId = 0;
            BufferSize = 0;
            BufferOffset = 0;
            BufferHandle = null;
            BufferManager = null;
        }

        internal abstract void Reset();
        internal abstract void Destroy();
        internal abstract void BufferAssigned();
    }
}
