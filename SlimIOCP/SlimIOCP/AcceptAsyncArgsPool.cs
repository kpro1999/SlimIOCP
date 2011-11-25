using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SlimIOCP
{
    internal class AcceptAsyncArgsPool : Pool
    {
        readonly Server server;
        readonly Stack<SocketAsyncEventArgs> pool;

        internal AcceptAsyncArgsPool(Server server)
            : this(server, 256)
        {

        }

        internal AcceptAsyncArgsPool(Server server, int preAllocateAmount)
        {
            this.server = server;

            pool = new Stack<SocketAsyncEventArgs>(preAllocateAmount);

            SocketAsyncEventArgs asyncArgs;

            for (var i = 0; i < preAllocateAmount; ++i)
            {
                if (TryAllocate(out asyncArgs))
                {
                    if (!TryPush(asyncArgs))
                    {
                        //TODO: Report error   
                    }
                }
                else
                {
                    //TODO: Report error   
                }
            }
        }

        public bool TryPush(SocketAsyncEventArgs asyncArgs)
        {
#if DEBUG
            if (asyncArgs == null)
            {
                //TODO: Error
                return false;
            }

            if (!(asyncArgs.UserToken is AcceptToken))
            {
                //TODO: Error
                return false;
            }
#endif

            // Clear the accept socket
            asyncArgs.AcceptSocket = null;

            lock (pool)
            {
                pool.Push(asyncArgs);
            }

            return true;
        }

        public bool TryPop(out SocketAsyncEventArgs asyncArgs)
        {
            if (pool.Count > 0)
            {
                lock (pool)
                {
                    if (pool.Count > 0)
                    {
                        asyncArgs = pool.Pop();
                        return true;
                    }
                }
            }

            return TryAllocate(out asyncArgs);
        }

        bool TryAllocate(out SocketAsyncEventArgs asyncArgs)
        {
            asyncArgs = new SocketAsyncEventArgs();
            asyncArgs.UserToken = new AcceptToken();
            asyncArgs.Completed += new EventHandler<SocketAsyncEventArgs>(server.OnAcceptCompleted);

            ++Allocated;

            return true;
        }
    }
}
