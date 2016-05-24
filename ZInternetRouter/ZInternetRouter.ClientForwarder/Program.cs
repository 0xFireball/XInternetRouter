using System;
using System.IO;
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

                var localProxyAddr = args[0];
                var localProxyPort = int.Parse(args[1]);
                //Subscribe to the ZIR proxy running locally


                //Parse remote endpoint
                var remoteAddr = args[2];
                var remotePort = int.Parse(args[3]);

                //Pause the thread neatly
                var suspendEvent = new ManualResetEvent(true);
                suspendEvent.Reset();

                //Start proxying loop, this is a service
                while (true)
                {
                    try
                    {
                        suspendEvent.Reset();
                        //Try to hold connections to remote as long as possible, and reconnect when necessary
                        var proxyClient = new TcpClient(localProxyAddr, localProxyPort); //Reconnect to ZIR proxy
                        var remoteConnectionClient = new TcpClient(remoteAddr, remotePort);
                        Console.WriteLine("Remote connected.");
                        var socketRouter = new SocketRoutingService();
                        //Make a 2-way route
                        socketRouter.CreateSocketRouteProxy(proxyClient.Client, remoteConnectionClient.Client, suspendEvent);
                        socketRouter.CreateSocketRouteProxy(remoteConnectionClient.Client, proxyClient.Client, suspendEvent);
                        //This is async, so pause the rest of the thread
                        suspendEvent.WaitOne();
                        //We have been signaled to reconnect!
                    }
                    catch (SocketException sEx)
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