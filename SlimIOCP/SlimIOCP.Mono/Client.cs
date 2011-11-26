using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SlimIOCP.Mono
{
    public class Client : Peer, IClient
    {
        public bool IsConnected
        {
            get;
            private set;
        }

        public void Connect(IPEndPoint endPoint)
        {
            InitSocket(endPoint);
            Socket.Connect(endPoint);
        }
    }
}
