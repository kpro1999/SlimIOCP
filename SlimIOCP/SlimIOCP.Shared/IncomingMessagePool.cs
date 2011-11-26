using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    internal class IncomingMessagePool
    {
        readonly BufferManager bufferManager;
        readonly Stack<IncomingMessage> pool;

        internal IncomingMessagePool(int preAllocateAmount)
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
                        break;
                    }
                }
                else
                {
                    //TODO: Error
                    break;
                }
            }
        }

        internal bool TryPush(IncomingMessage message)
        {
#if DEBUG
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
#endif

            message.Reset();

            lock (pool)
            {
                pool.Push(message);
            }

            return true;
        }

        internal bool TryPop(out IncomingMessage message)
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
                message = new IncomingMessage(this);
                message.SetBuffer(bufferHandle, bufferId, bufferOffset, bufferSize);
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
