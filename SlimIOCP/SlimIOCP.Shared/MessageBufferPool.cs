using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    internal class MessageBufferPool<T> where T : MessageBuffer
    {
        int clearBufferId = -1;
        readonly int poolAmount;

        readonly Queue<T> pool;
        readonly BufferManager bufferManager;
        readonly MessageBufferProducer<T> producer;

        internal MessageBufferPool(MessageBufferProducer<T> messageProducer)
            : this(messageProducer, 5, 1024, 1024, 1024)
        {

        }

        internal MessageBufferPool(MessageBufferProducer<T> messageProducer, int maxBuffers, int chunkSize, int chunksPerBlock, int maxPooled)
        {
#if DEBUG
            if (messageProducer == null)
            {
                throw new ArgumentNullException("messageProducer");
            }
#endif

            poolAmount = maxPooled;
            pool = new Queue<T>(maxPooled);
            producer = messageProducer;
            bufferManager = new BufferManager(maxBuffers, chunkSize, chunksPerBlock);

            // Pre-allocate the amount of maxPooled objects we want to have

            T preAllocated;

            for (var i = 0; i < poolAmount; ++i)
            {
                if (tryAllocate(out preAllocated))
                {
                    if (!TryPush(preAllocated))
                    {
                        //TODO: Error
                    }
                }
                else
                {
                    //TODO: Error
                }
            }
        }

        internal bool TryPop(out T message)
        {
            lock (pool)
            {
                if (pool.Count > 0)
                {
                    message = pool.Dequeue();
                    return true;
                }
            }

            return tryAllocate(out message);
        }

        internal bool TryPush(T message)
        {
#if DEBUG
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (!Object.ReferenceEquals(message.BufferManager, bufferManager))
            {
                throw new ArgumentException("Message does not belong to this pool", "message");
            }
#endif

            if (clearBufferId == message.BufferId)
            {
                message.Destroy();
                message.ClearBuffer();
                producer.Return(message);
            }
            else
            {
                message.Reset();

                lock (pool)
                {
                    pool.Enqueue(message);
                }
            }

            return true;
        }

        bool tryAllocate(out T message)
        {
            int bufferId;
            int bufferOffset;
            int bufferSize;
            byte[] bufferHandle;

            if (bufferManager.TryAllocateBuffer(out bufferId, out bufferOffset, out bufferSize, out bufferHandle))
            {
                message = producer.Get();
                message.SetBuffer(bufferManager, bufferHandle, bufferId, bufferOffset, bufferSize);
                message.BufferAssigned();
                return true;
            }

            message = null;
            return false;
        }
    }
}
