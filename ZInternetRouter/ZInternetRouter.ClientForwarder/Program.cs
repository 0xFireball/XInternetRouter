using System;
using System.Net.Sockets;
using System.Threading;
using ZInternetRouter.Server.Core;

namespace ZInternetRouter.ClientForwarder
{
    internal class Program
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
                var wait = false;
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

                //Pause the thread neatly
                var suspendEvent = new ManualResetEvent(true);
                suspendEvent.Reset();

                //Start proxying loop, this is a service
                while (true)
                {
                    try
                    {
                        var localProxyAddr = args[0];
                        var localProxyPort = int.Parse(args[1]);
                        var proxyClient = new TcpClient(localProxyAddr, localProxyPort);
                        //Subscribe to the ZIR proxy running locally
                        //Try to hold connections to remote as long as possible, and reconnect when necessary
                        var remoteAddr = args[2];
                        var remotePort = int.Parse(args[3]);
                        var remoteConnectionClient = new TcpClient(remoteAddr, remotePort);
                        var socketRouter = new SocketRoutingService();
                        socketRouter.CreateSocketRouteProxy(proxyClient.Client, remoteConnectionClient.Client);
                        //This is async, so pause the rest of the thread
                        while (true)
                        {
                            suspendEvent.WaitOne();
                        }
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Remote disconnected.");
                    }
                }
            }
        }

        public static void ShowUsage()
        {
            Console.WriteLine(
                "Usage:\nClientForwarder <local proxy address> <local proxy port> <remote address> <remote port>");
        }
    }
}