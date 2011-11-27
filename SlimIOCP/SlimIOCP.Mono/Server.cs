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
            throw new NotImplementedException();
        }
    }
}
