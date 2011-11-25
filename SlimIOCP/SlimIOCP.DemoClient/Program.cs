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
        static void Thread(object state)
        {
            //var buffer = new byte[1024 * 1024];
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 14000);
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(endPoint);

            var data = new byte[256];
            var tosend = 18;
            var sent = 0;

            while (true)
            {
                tosend = 18;

                while (tosend > 0)
                {
                    
                    tosend -= socket.Send(data, tosend, SocketFlags.None);

                }

                ++sent;

                if (sent == 1)
                {
                    sent = 0;
                }
            }
        }

        static byte[] data = new byte[256];

        static Program()
        {
            var size = BitConverter.GetBytes((ushort)(data.Length - 2));
            Array.Copy(size, data, 2);
        }

        static void Send(Socket socket)
        {
            Interlocked.Increment(ref sent);
            Interlocked.Decrement(ref canSend);
            socket.BeginSend(data, 0, 256, SocketFlags.None, SendDone, socket);
        }

        static void SendDone(IAsyncResult result)
        {
            var socket = (Socket)result.AsyncState;
            socket.EndSend(result);
            Interlocked.Increment(ref canSend);
        }

        static int sent = 0;
        static int canSend = 1024;

        static void Main(string[] args)
        {
            List<Socket> sockets = new List<Socket>();

            for (var i = 0; i < 1024; ++i)
            {
                var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 14000);
                var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(endPoint);
                sockets.Add(socket);
            }

            var sw = new System.Diagnostics.Stopwatch();
            var time = DateTime.Now;

            sw.Start();

            while (true)
            {
                foreach (var socket in sockets)
                {
                    Send(socket);
                }

                System.Threading.Thread.Sleep(100);

                if ((DateTime.Now - time).Seconds > 1)
                {
                    time = DateTime.Now;
                    Console.WriteLine("Messages/Second: " + ((float)sent / ((float)sw.ElapsedMilliseconds / (float)1000)));
                }
            }

            Console.ReadLine();

            /*
            var recv = socket.Receive(buffer);
            var length = BitConverter.ToUInt16(buffer, 0);

            Console.WriteLine("Got: " + length);
            Console.Write("Press [Enter] to exit");
            Console.ReadLine();
            */
        }
    }
}
