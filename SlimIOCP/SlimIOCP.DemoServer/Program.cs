using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SlimIOCP.DemoServer
{
    class Program
    {
        string test = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus interdum ultricies lectus, in tincidunt est euismod ut. In facilisis dignissim feugiat. In vestibulum commodo purus. Curabitur vulputate porta pulvinar. Nullam dapibus, nisl non congue aliquet, mauris purus elementum velit, id pellentesque metus massa eget ligula. Vestibulum interdum odio vehicula nulla sagittis sit amet porttitor justo cursus. Morbi congue laoreet metus, imperdiet rhoncus enim ullamcorper eu. Nunc hendrerit aliquam neque, sed feugiat augue placerat nec. Phasellus ut velit ipsum. Donec id dolor elit. Mauris et blandit nulla. Morbi ullamcorper viverra sodales. Nulla sodales massa at neque laoreet aliquet. Donec sollicitudin, elit vitae porttitor suscipit, sapien arcu hendrerit tellus, eget mattis nisl nibh vel risus. Donec sit amet magna augue. Pellentesque varius condimentum sapien vitae scelerisque.

Morbi convallis sem non tellus tempus malesuada. Morbi scelerisque nulla at sapien cursus posuere. Quisque quis lectus mauris. Integer augue odio, venenatis in euismod id, tempus dapibus libero. Nam convallis rhoncus luctus. Quisque ut risus massa, id placerat massa. Etiam volutpat mattis interdum. Aliquam erat volutpat. Sed aliquet semper metus nec dictum. Donec congue tristique adipiscing. Pellentesque sem metus, rhoncus sit amet semper id, egestas at ante. Fusce libero erat, tempus vestibulum bibendum id, varius non tellus. Proin consectetur sapien non leo blandit tempus eu id lorem. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras nec nibh vel felis mattis pretium.

Proin turpis nunc, tristique sed mollis ut, blandit quis nunc. Quisque dignissim auctor sem sit amet scelerisque. Cras euismod accumsan vestibulum. Proin tristique odio nec neque mollis pharetra. Mauris et elit velit. Cras tincidunt eros sed sapien feugiat sed blandit lacus feugiat. Aenean tempor pellentesque augue, eu interdum est porta vitae.

Pellentesque venenatis magna vel lectus placerat quis interdum odio bibendum. Morbi a hendrerit turpis. Aliquam faucibus gravida tincidunt. Vestibulum a faucibus turpis. Morbi ut magna nibh, ut vulputate diam. Integer scelerisque condimentum magna ac aliquam. Suspendisse sit amet augue in urna tristique placerat. Integer at lacus at tellus fermentum semper sed a nibh.

Suspendisse eu erat nec dui blandit placerat id eu sem. Ut porta orci vitae augue molestie tempor. In dapibus neque at nisi ultricies sit amet mollis magna ornare. Aenean accumsan purus quis magna dignissim sagittis. Curabitur erat sem, dapibus in mollis in, viverra id orci. Phasellus porttitor elit quis urna suscipit sit amet luctus sapien pulvinar. Sed bibendum congue est, quis tincidunt sem mattis vel. Donec pretium, ipsum ut porttitor porttitor, massa lacus mollis magna, in lacinia lacus tellus euismod justo. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Pellentesque nec felis ac massa mattis fringilla. Cras imperdiet felis et nunc volutpat id lacinia enim viverra. Pellentesque lectus lorem, faucibus in hendrerit ac, pulvinar ac metus.";

        static void Main(string[] args)
        {
            var server = new SlimIOCP.Win32.Server();
            server.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 14000));

            IncomingMessage message;

            while (true)
            {
                while (server.TryGetMessage(out message))
                {
                    server.TryRecycleMessage(message);
                }

                server.ReceivedMessageEvent.WaitOne();
            }
        }
    }
}
