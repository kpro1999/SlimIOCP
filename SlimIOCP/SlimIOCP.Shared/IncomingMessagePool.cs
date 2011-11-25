using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    public class IncomingMessagePool : Pool
    {
        readonly BufferManager bufferManager;
        readonly Stack<IncomingMessage> pool;

        public IncomingMessagePool(int preAllocateAmount)
        {
            pool = new Stack<IncomingMessage>();
            bufferManager = new BufferManager(5, 1024, preAllocateAmount * 2);

            IncomingMessage message;

            for (var i = 0; i < preAllocateAmount; ++i)
            {
                if (TryAllocate(out message))
                {
                    if (!TryPush(message))
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

        public bool TryPush(IncomingMessage message)
        {
            message.Length = 0;
            message.BytesRead = 0;
            message.BytesRemaining = 0;
            message.Header.Short = 0;
            message.HeaderBytesReceived = 0;
            message.IsDone = false;

            lock (pool)
            {
                pool.Push(message);
            }

            ++Pooled;
            return true;
        }

        public bool TryPop(out IncomingMessage message)
        {
            lock (pool)
            {
                if (pool.Count > 0)
                {
                    message = pool.Pop();
                    return true;
                }
            }

            return TryAllocate(out message);
        }

        bool TryAllocate(out IncomingMessage message)
        {
            int bufferId;
            int bufferOffset;
            int bufferSize;
            byte[] bufferHandle;

            if (bufferManager.TryAllocateBuffer(out bufferId, out bufferOffset, out bufferSize, out bufferHandle))
            {
                message = new IncomingMessage(this, bufferId, bufferOffset, bufferSize, bufferHandle);
                Allocated++;
                return true;
            }
            else
            {
                //TODO: Error
                message = null;
                return false;
            }
        }
    }
}
