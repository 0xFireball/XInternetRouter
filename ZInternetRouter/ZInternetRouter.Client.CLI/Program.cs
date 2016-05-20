using System;
using System.IO;
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
            string partnerId = ConsoleExtensions.ReadWrite("Enter a partner ID: ");
            outputStream.WriteLine("getroute");
            outputStream.Flush();
            outputStream.WriteLine(partnerId);
            outputStream.Flush();
            string msg = inputStream.ReadLine();
            Console.WriteLine(msg);
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Console.WriteLine(inputStream.ReadLine());
                }
            });
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