using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    public class BufferManager
    {
        public readonly int ChunkSize;
        public readonly int ChunksPerBuffer;
        public readonly int MaxBuffers;

        Buffer currentBuffer;

        readonly object syncObject = new object();
        readonly List<Buffer> buffers = new List<Buffer>();

        public BufferManager(int maxBuffers, int chunkSize, int chunksPerBuffer)
        {
            MaxBuffers = maxBuffers;
            ChunkSize = chunkSize;
            ChunksPerBuffer = chunksPerBuffer;
            initNewBuffer();
        }

        public bool TryAllocateBuffer(out int id, out int offset, out int size, out byte[] handle)
        {
            lock (syncObject)
            {
                if (!currentBuffer.TryAllocateBuffer(out offset, out size, out handle))
                {
                    selectNewCurrentBuffer();

                    if (!currentBuffer.TryAllocateBuffer(out offset, out size, out handle))
                    {
                        //TODO: Error
                        id = 0;
                        return false;
                    }
                }

                id = currentBuffer.BufferId;
                return true;
            }
        }

        void initNewBuffer()
        {
            buffers.Add(currentBuffer = new Buffer(buffers.Count, ChunkSize, ChunksPerBuffer));
        }

        void selectNewCurrentBuffer()
        {
            for (var i = 0; i < buffers.Count; ++i)
            {
                if (buffers[i].FreeBuffers > 16)
                {
                    currentBuffer = buffers[i];
                    return;
                }
            }

            initNewBuffer();
        }
    }
}
