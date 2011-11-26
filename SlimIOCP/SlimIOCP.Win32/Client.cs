using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SlimIOCP.Win32
{
    public class Client : Peer, IClient
    {
        public List<Connection> AllConnections
        {
            get { return Connections; }
        }

        public bool IsConnected
        {
            get;
            private set;
        }

        public void Connect(IPEndPoint endPoint)
        {
            Socket = null;

            InitSocket(endPoint);
            Socket.Connect(endPoint);
            IsConnected = true;

            var connection = new Connection(this);
            connection.Socket = Socket;

            Receive(connection);

            Connections.Add(connection);
        }
    }
}
