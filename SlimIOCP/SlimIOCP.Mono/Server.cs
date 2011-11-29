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
            get { return Connections.Count; }
        }

        public void Start(IPEndPoint endPoint)
        {
            InitSocket(endPoint);

            Socket.Bind(endPoint);
            Socket.Listen(100);

            SlimCommon.Log.Default.Info("[SlimIOCP] Listening on " + endPoint);

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
                SlimCommon.Log.Default.Info("[SlimIOCP] Client connected from " + clientSocket.RemoteEndPoint);

                Connections.Add(connection);
                connection.Socket = clientSocket;
                connection.Connected = true;

                Receive(connection);

                PushMessage(MetaMessagePool.Pop(MessageType.Connected, connection));
            }
            else
            {
                //TODO: Error
            }

            Accept();
        }
    }
}
