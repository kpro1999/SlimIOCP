using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SlimIOCP
{
    internal class Buffer
    {
        internal readonly int BufferId;
        internal readonly int ChunkSize;
        internal readonly int ChunkCount;

        readonly byte[] storage;
        readonly Stack<int> freeOffsets = new Stack<int>();
        readonly HashSet<int> usedOffsets = new HashSet<int>();

        internal int FreeBuffers { get { return freeOffsets.Count; } }

        internal Buffer(int bufferId, int chunkSize, int chunkCount)
        {
            BufferId = bufferId;
            ChunkSize = chunkSize;
            ChunkCount = chunkCount;

            storage = new byte[chunkSize * chunkCount];

            for (var i = (chunkCount-1); i >= 0; --i)
            {
                freeOffsets.Push(i * chunkSize);
            }
        }

        internal bool TryAllocateBuffer(out int offset, out int size, out byte[] handle)
        {
            if (freeOffsets.Count > 0)
            {
                size = ChunkSize;
                offset = freeOffsets.Pop();
                handle = storage;

                usedOffsets.Add(offset);

                return true;
            }

            //TODO: Error
            size = 0;
            offset = 0;
            handle = null;
            return false;
        }

        internal bool TryReturnBuffer(int index)
        {
            if (usedOffsets.Contains(index))
            {
                freeOffsets.Push(index);
                usedOffsets.Remove(index);
                return true;
            }

            //TODO: Error
            return false;
        }
    }
}
