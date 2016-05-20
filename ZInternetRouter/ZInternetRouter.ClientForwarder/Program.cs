using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZInternetRouter.Server.Core;

namespace ZInternetRouter.ClientForwarder
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ZInternetRouter Proxy Forwarder");
            Console.WriteLine("(c) 0xFireball, 2016. All Rights Reserved.");
            if (args.Length < 4)
            {
                ShowUsage();
            }
            else
            {
                bool wait = false;
                if (args.Length >= 5)
                {
                    if (args[4] == "-wait")
                        wait = true;
                }
                if (wait)
                {
                    Console.WriteLine("Waiting for ENTER");
                    Console.ReadLine();
                }
                var localProxyAddr = args[0];
                var localProxyPort = int.Parse(args[1]);
                var proxyClient = new TcpClient(localProxyAddr, localProxyPort);
                var remoteAddr = args[2];
                var remotePort = int.Parse(args[3]);
                var routingProxy = new InternetRoutingProxy();
                routingProxy.StartProxy(proxyClient.Client, new IPEndPoint(IPAddress.Parse(remoteAddr), remotePort));
                ManualResetEvent suspendEvent = new ManualResetEvent(true);
                suspendEvent.Reset();
                while (true)
                {
                    suspendEvent.WaitOne();
                }
            }
        }
        public static void ShowUsage()
        {
            Console.WriteLine("Usage:\nClientForwarder <local proxy address> <local proxy port> <remote address> <remote port>");
        }
    }
}
