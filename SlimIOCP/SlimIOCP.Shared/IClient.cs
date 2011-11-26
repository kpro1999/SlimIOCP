using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SlimIOCP
{
    public interface IClient
    {
        bool IsConnected { get; }
        void Connect(IPEndPoint endPoint);
    }
}
