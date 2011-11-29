using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SlimIOCP.DemoClient
{
    class Program
    {
        static int sent = 1;
        static int recv = 1;

        static void Main(string[] args)
        {
            SlimCommon.Log.Default.Logger = new SlimCommon.ConsoleLogger();

            var clients = new List<SlimIOCP.Mono.Client>();
            var started = false;

            for (var i = 0; i < 1; ++i)
            {
                var endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.10"), 14000);
                var client = new SlimIOCP.Mono.Client();

                for (var j = 0; j < 1; ++j)
                {
                    client.Connect(endPoint);
                }

                clients.Add(client);
            }

            var sw = new System.Diagnostics.Stopwatch();
            var time = DateTime.Now;

            sw.Start();

            restart:
            if (started)
            {
                clients[0].Disconnect(new IPEndPoint(IPAddress.Parse("192.168.0.10"), 14000));
                clients[0].Connect(new IPEndPoint(IPAddress.Parse("192.168.0.10"), 14000));
            }

            started = true;

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
                            /*
                            outgoingMessage.TryWrite(blah);
                            outgoingMessage.TryWrite(blah);
                            */

                            if (!outgoingMessage.TryWrite(BitConverter.GetBytes(sent)))
                            {
                                throw new Exception();
                            }
                            else
                            {
                                Console.WriteLine("Sent: " + sent + " to server");
                            }

                            if (!outgoingMessage.TryQueue())
                            {
                                throw new Exception();
                            }

                            ++sent;
                        }

                        while (client.TryPopMessage(out incommingMessage))
                        {
                            if (incommingMessage.MessageType != MessageType.Data)
                                continue;

                            ++recv;

                            var recvBack = BitConverter.ToInt32(incommingMessage.Buffer, incommingMessage.Offset);
                            Console.WriteLine("Recv: " + recvBack + " from server");

                            client.TryRecycleMessage(incommingMessage);
                        }
                    }

                    if ((DateTime.Now - time).Seconds > 2)
                    {
                        time = DateTime.Now;
                        Console.WriteLine("Messages/Second Out: " + ((float)sent / ((float)sw.ElapsedMilliseconds / (float)1000)));
                        goto restart;
                    }
                }

                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
