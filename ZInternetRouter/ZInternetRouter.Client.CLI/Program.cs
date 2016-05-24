using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ZInternetRouter.Server.Core;

namespace ZInternetRouter.Client.CLI
{
    internal class Program
    {
        #region Private Methods

        private static void Main(string[] args)
        {
            var zirClient = new TcpClient(args[0], int.Parse(args[1]));
            var inputStream = new StreamReader(zirClient.GetStream());
            var outputStream = new StreamWriter(new BufferedStream(zirClient.GetStream()));
            string memberId = inputStream.ReadLine();
            Console.WriteLine("Connected to server!");
            Console.WriteLine($"My ID: {memberId}");

            //Proxy client
            int proxyPort = int.Parse(args[2]);
            Console.WriteLine($"Proxy running on port {proxyPort}");
            var routingProxy = new InternetRoutingProxy();
            routingProxy.StartProxy(new IPEndPoint(IPAddress.Any, proxyPort), zirClient.Client);
        }

        #endregion Private Methods
    }
}