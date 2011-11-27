using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlimIOCP.Mono
{
    internal class ConnectionPool
    {
        Peer peer;
        Stack<Connection> pool = new Stack<Connection>();

        internal ConnectionPool(Peer peer)
            : this(peer, 1024)
        {

        }

        internal ConnectionPool(Peer peer, int preAllocateAmount)
        {
#if DEBUG
            if (peer == null)
            {
                throw new ArgumentNullException("peer");
            }
#endif

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
            if (pool.Count > 0)
            {
                lock (pool)
                {
                    if (pool.Count > 0)
                    {
                        connection = pool.Pop();

                        return true;
                    }
                }
            }

            return TryAllocate(out connection);
        }

        public bool TryPush(Connection connection)
        {
#if DEBUG
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (connection.SendQueue.Count > 0)
            {
                //TODO: Error
                return false;
            }
#endif

            connection.Reset();

            lock (pool)
            {
                pool.Push(connection);
            }

            return true;
        }

        bool TryAllocate(out Connection connection)
        {
            connection = new Connection(peer);
            return true;
        }
    }
}
