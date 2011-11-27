using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SlimIOCP.Mono;

namespace SlimIOCP.DemoClient
{
    class Program
    {
        static int sent = 0;
        static int recv = 0;
        static byte[] data = new byte[128];

        static void Main(string[] args)
        {
            var clients = new List<Client>();

            for (var i = 0; i < 8; ++i)
            {
                var endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.10"), 14000);
                var client = new Client();

                for (var j = 0; j < 128; ++j)
                {
                    client.Connect(endPoint);
                }

                clients.Add(client);
            }

            var sw = new System.Diagnostics.Stopwatch();
            var time = DateTime.Now;

            sw.Start();

            while (true)
            {
                SlimIOCP.Mono.OutgoingMessage outgoingMessage;
                SlimIOCP.Mono.IncomingMessage incommingMessage;

                foreach (var client in clients)
                {
                    foreach (var connection in client.AllConnections)
                    {
                        if (connection.TryCreateMessage(out outgoingMessage))
                        {
                            if (!outgoingMessage.TryWrite(data))
                            {
                                throw new Exception();
                            }

                            if (!outgoingMessage.TryQueue())
                            {
                                throw new Exception();
                            }

                            ++sent;
                        }

                        while (client.TryGetMessage(out incommingMessage))
                        {
                            ++recv;
                            client.TryRecycleMessage(incommingMessage);
                        }
                    }

                    if ((DateTime.Now - time).Seconds > 1)
                    {
                        time = DateTime.Now;
                        Console.WriteLine("Messages/Second Out: " + ((float)sent / ((float)sw.ElapsedMilliseconds / (float)1000)));
                    }
                }

                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
