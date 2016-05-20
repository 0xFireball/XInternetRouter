using System;
using System.Net.Sockets;

namespace ZInternetRouter.Server.Core
{
    public class SocketRoutingService
    {
        /// <summary>
        /// Creates a route between two sockets, acting as a proxy.
        /// </summary>
        /// <param name="socket1"></param>
        /// <param name="socket2"></param>
        public void CreateSocketRouteProxy(Socket socket1, Socket socket2)
        {
            var forwardingInformation1 = new ForwardingInfo(socket1, socket2);
            socket1.BeginReceive(forwardingInformation1.Buffer, 0, forwardingInformation1.Buffer.Length, SocketFlags.None, OnDataReceive, forwardingInformation1);
        }

        private static void OnDataReceive(IAsyncResult result)
        {
            var forwardingInformation = (ForwardingInfo)result.AsyncState;
            try
            {
                var bytesRead = forwardingInformation.SourceSocket.EndReceive(result);
                if (bytesRead > 0)
                {
                    forwardingInformation.DestinationSocket.Send(forwardingInformation.Buffer, bytesRead, SocketFlags.None);
                    forwardingInformation.SourceSocket.BeginReceive(forwardingInformation.Buffer, 0, forwardingInformation.Buffer.Length, 0, OnDataReceive, forwardingInformation);
                }
            }
            catch (SocketException)
            {
                forwardingInformation.DestinationSocket.Close();
                forwardingInformation.SourceSocket.Close();
            }
        }
    }
}