using System;
using System.Net;
using System.Net.Sockets;

namespace ZInternetRouter.Server.Core
{
    /// <summary>
    /// A class to proxy a Socket connection
    /// </summary>
    public class InternetRoutingProxy
    {
        private readonly Socket _baseSocket;

        public InternetRoutingProxy()
        {
            _baseSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Proxy through a TCP socket
        }

        private InternetRoutingProxy(Socket existingConnectionToRemoteSocket)
        {
            _baseSocket = existingConnectionToRemoteSocket;
        }

        /// <summary>
        /// Starts the proxy between existingLocalConnection and remoteEndPoint
        /// </summary>
        /// <param name="localEndPoint">The local endpoint of the tunnel</param>
        /// <param name="remoteEndPoint">The remote endpoint of the tunnel</param>
        public void StartProxy(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            _baseSocket.Bind(localEndPoint);
            _baseSocket.Listen(10);

            while (true)
            {
                var source = _baseSocket.Accept();
                var destination = new InternetRoutingProxy();
                var forwardingInformation = new ForwardingInfo(source, destination._baseSocket);
                destination.Connect(remoteEndPoint, source);
                source.BeginReceive(forwardingInformation.Buffer, 0, forwardingInformation.Buffer.Length, 0, OnDataReceive, forwardingInformation);
            }
        }

        public void StartProxy(IPEndPoint localEndPoint, Socket existingRemoteConnection)
        {
            _baseSocket.Bind(localEndPoint);
            _baseSocket.Listen(10);

            while (true)
            {
                try
                {
                    var source = _baseSocket.Accept();
                    var destination = new InternetRoutingProxy(existingRemoteConnection);
                    var forwardingInformation = new ForwardingInfo(source, destination._baseSocket);
                    destination.ConnectWithExisting(source);
                    source.BeginReceive(forwardingInformation.Buffer, 0, forwardingInformation.Buffer.Length, 0, OnDataReceive, forwardingInformation);
                }
                catch (Exception)
                {
                    //Something bad happened. Too bad.
                }
            }
        }

        public void StartProxy(Socket existingLocalConnection, IPEndPoint remoteEndPoint)
        {
            var source = existingLocalConnection;
            var destination = new InternetRoutingProxy();
            var forwardingInformation = new ForwardingInfo(source, destination._baseSocket);
            destination.Connect(remoteEndPoint, source);
            source.BeginReceive(forwardingInformation.Buffer, 0, forwardingInformation.Buffer.Length, 0, OnDataReceive, forwardingInformation);
        }

        private void Connect(EndPoint remoteEndpoint, Socket destination)
        {
            var forwardingInformation = new ForwardingInfo(_baseSocket, destination);
            _baseSocket.Connect(remoteEndpoint);
            _baseSocket.BeginReceive(forwardingInformation.Buffer, 0, forwardingInformation.Buffer.Length, SocketFlags.None, OnDataReceive, forwardingInformation);
        }

        private void ConnectWithExisting(Socket destination)
        {
            var forwardingInformation = new ForwardingInfo(_baseSocket, destination);
            _baseSocket.BeginReceive(forwardingInformation.Buffer, 0, forwardingInformation.Buffer.Length, SocketFlags.None, OnDataReceive, forwardingInformation);
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
            catch (Exception)
            {
                forwardingInformation.DestinationSocket.Close();
                forwardingInformation.SourceSocket.Close();
            }
        }
    }
}