using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SlimIOCP
{
    internal class Buffer
    {
        readonly byte[] storage;

        internal readonly int BufferId;
        internal readonly int ChunkSize;
        internal readonly int ChunkCount;
        internal readonly Stack<int> FreeOffsets = new Stack<int>();
        internal readonly HashSet<int> UsedOffsets = new HashSet<int>();

        internal Buffer(int bufferId, int chunkSize, int chunkCount)
        {
            BufferId = bufferId;
            ChunkSize = chunkSize;
            ChunkCount = chunkCount;

            storage = new byte[chunkSize * chunkCount];

            for (var i = (chunkCount-1); i >= 0; --i)
            {
                FreeOffsets.Push(i * chunkSize);
            }
        }

        internal bool TryAllocateBuffer(out int offset, out int size, out byte[] handle)
        {
            if (FreeOffsets.Count > 0)
            {
                size = ChunkSize;
                offset = FreeOffsets.Pop();
                handle = storage;

                UsedOffsets.Add(offset);

                return true;
            }

            //TODO: Error
            size = 0;
            offset = 0;
            handle = null;
            return false;
        }

        internal bool TryReturnBuffer(int offset)
        {
            if (UsedOffsets.Contains(offset))
            {
                FreeOffsets.Push(offset);
                UsedOffsets.Remove(offset);
                return true;
            }
#if DEBUG
            else
            {
                throw new ArgumentException("Offset is not in use", "id");
            }
#endif

            //TODO: Error
            return false;
        }
    }
}
