using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SlimIOCP.Mono
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

        public void Disconnect(IPEndPoint endPoint)
        {
            foreach (var connection in Connections)
            {
                if (connection.RemoteEndPoint.Address.Equals(endPoint.Address) && connection.RemoteEndPoint.Port == endPoint.Port)
                {
                    Disconnect(connection);
                    break;
                }
            }

            SlimCommon.Log.Default.Info("Connections.Count: " + Connections.Count);

            if (Connections.Count == 0)
            {
                Receiver.Running = false;
            }
        }

        public void Connect(IPEndPoint endPoint)
        {
            Socket = null;

            InitSocket(endPoint);
            Socket.Connect(endPoint);
            IsConnected = true;

            var connection = new Connection(this);
            connection.Socket = Socket;
            connection.Connected = true;

            Receive(connection);

            Connections.Add(connection);

            PushMessage(MetaMessagePool.Pop(MessageType.Connected, connection));
        }
    }
}
