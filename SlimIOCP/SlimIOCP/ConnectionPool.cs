using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP
{
    internal class ConnectionPool : Pool
    {
        Peer peer;
        Stack<Connection> pool = new Stack<Connection>();

        internal ConnectionPool(Peer peer)
            : this(peer, 1024)
        {

        }

        internal ConnectionPool(Peer peer, int preAllocateAmount)
        {
            this.peer = peer;

            Connection connection;

            for (var i = 0; i < preAllocateAmount; ++i)
            {
                if (TryAllocate(out connection))
                {
                    if (!TryPush(connection))
                    {
                        //TODO: Error
                    }
                }
                else
                {
                    //TODO: Error
                    break;
                }
            }
        }

        public bool TryPop(out Connection connection)
        {
            if (Pooled > 0)
            {
                lock (pool)
                {
                    if (Pooled > 0)
                    {
                        connection = pool.Pop();
                        connection.Pooled = false;

                        --Pooled;
                        return true;
                    }
                }
            }

            return TryAllocate(out connection);
        }

        public bool TryPush(Connection connection)
        {
#if DEBUG
            if (connection.SendQueue.Count > 0)
            {
                //TODO: Error
                return false;
            }
#endif
            connection.Socket = null;
            connection.IsQueued = false;
            connection.Pooled = true;

            lock (pool)
            {
                pool.Push(connection);
            }

            ++Pooled;
            return true;
        }

        bool TryAllocate(out Connection connection)
        {
            connection = new Connection(peer);
            connection.Pooled = false;

            ++Allocated;
            return true;
        }
    }
}
