using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SlimIOCP.Mono
{
    public class Server : Peer, IServer
    {
        public int ConnectedClients
        {
            get { throw new NotImplementedException(); }
        }

        public void Start(IPEndPoint endPoint)
        {
            InitSocket(endPoint);

            Socket.Bind(endPoint);
            Socket.Listen(100);

            Accept();
        }

        void Accept()
        {
            Socket.BeginAccept(AcceptDone, null);
        }

        void AcceptDone(IAsyncResult result)
        {
            var clientSocket = Socket.EndAccept(result);

            Connection connection;

            if (ConnectionPool.TryPop(out connection))
            {
                Connections.Add(connection);
                connection.Socket = clientSocket;

                Receive(connection);

                TryPushMessage(MetaMessagePool.Pop(MessageType.Connected, connection));
            }
            else
            {
                //TODO: Error
            }

            Accept();
        }
    }
}
