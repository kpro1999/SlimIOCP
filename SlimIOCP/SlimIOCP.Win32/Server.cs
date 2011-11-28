using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SlimIOCP.Win32
{
    public class Server : Peer, IServer
    {
        readonly AcceptAsyncArgsPool acceptAsyncArgsPool;

        public int ConnectedClients { get { return Connections.Count; } }

        public Server()
        {
            acceptAsyncArgsPool = new AcceptAsyncArgsPool(this);
        }

        public void Start(IPEndPoint endPoint)
        {
            InitSocket(endPoint);

            Socket.Bind(endPoint);
            Socket.Listen(100);

            AcceptAsync();
        }

        void AcceptAsync()
        {
            SocketAsyncEventArgs asyncArgs;

            if (acceptAsyncArgsPool.TryPop(out asyncArgs))
            {
                var isDone = !Socket.AcceptAsync(asyncArgs);
                if (isDone)
                {
                    OnAccept(asyncArgs);
                }
            }
            else
            {
                //TODO: Error
            }
        }

        void OnAccept(SocketAsyncEventArgs asyncArgs)
        {
            if (asyncArgs.SocketError != SocketError.Success)
            {
                OnAcceptError(asyncArgs);
                return;
            }

            Connection connection;

            if (ConnectionPool.TryPop(out connection))
            {
                Connections.Add(connection);
                connection.Socket = asyncArgs.AcceptSocket;

                Receive(connection);

                PushMessage(MetaMessagePool.Pop(MessageType.Connected, connection));
            }
            else
            {
                //TODO: Error
            }

            AcceptAsync();
        }

        void OnAcceptError(SocketAsyncEventArgs asyncArgs)
        {
            var acceptToken = (asyncArgs.UserToken as AcceptToken);
            asyncArgs.AcceptSocket.Close();
        }

        internal void OnAcceptCompleted(object sender, SocketAsyncEventArgs asyncArgs)
        {
            OnAccept(asyncArgs);
        }
    }
}
