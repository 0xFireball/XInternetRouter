using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

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
            int proxyPort = 24999;
            TcpListener proxyListener = new TcpListener(IPAddress.Any, proxyPort);
            List<TcpClient> proxyClients = new List<TcpClient>();
            Task.Factory.StartNew(() => //Proxy thread
            {
                proxyListener.Start();
                while (true)
                {
                    var s = proxyListener.AcceptTcpClient();
                    proxyClients.Add(s);
                }
            });
            /*
            Task.Factory.StartNew(() => //Thread to display output
            {
                while (true)
                {
                    Console.WriteLine(inputStream.ReadLine());
                }
            });
            */
            while (true)
            {
                var dts = Console.ReadLine();
                outputStream.WriteLine(dts);
                outputStream.FlushAsync();
            }
        }

        #endregion Private Methods
    }
}