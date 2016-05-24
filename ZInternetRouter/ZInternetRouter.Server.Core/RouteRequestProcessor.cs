using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace ZInternetRouter.Server.Core
{
    public class RouteRequestProcessor
    {
        #region Public Constructors

        public RouteRequestProcessor(TcpClient baseSocket, ProxyRoutingConnectorService proxyRoutingConnectorService)
        {
            _baseSocket = baseSocket;
            _proxyRoutingConnectorService = proxyRoutingConnectorService;
        }

        #endregion Public Constructors

        #region Private Fields

        private readonly TcpClient _baseSocket;
        private readonly ProxyRoutingConnectorService _proxyRoutingConnectorService;
        private bool _stayConnected;

        #endregion Private Fields

        #region Public Methods

        public void KillConnection()
        {
            _baseSocket.Close();
            _stayConnected = false;
        }

        public void ProcessConnection()
        {
            InputStream = new StreamReader(_baseSocket.GetStream());
            OutputStream = new StreamWriter(new BufferedStream(_baseSocket.GetStream()));
            try
            {
                OutputStream.WriteLine(HostClient.MemberId);
                OutputStream.Flush();
                //Impatiently wait for data
                //_baseSocket.ReceiveTimeout = 10000;
                Console.WriteLine($"Client {HostClient.MemberId} connected.");
                while (ReceivingCommands)
                {
                    var command = InputStream.ReadLine();
                    Console.WriteLine($"Client {HostClient.MemberId} sent command {command}");
                    switch (command)
                    {
                        case "setid":
                            var id = InputStream.ReadLine();
                            HostClient.MemberId = id;
                            break;

                        case "getroute":
                            var targetRouteId = InputStream.ReadLine();
                            var targetCandidates =
                                _proxyRoutingConnectorService.ConnectedClients.Where(cc => cc.MemberId == targetRouteId)
                                    .ToList();
                            if (targetCandidates.Count > 0)
                            {
                                OutputStream.WriteLine("SUCCESS");
                                OutputStream.Flush();
                                var routingController = new SocketRoutingService();
                                var routeTargetClient = targetCandidates[0];
                                ReceivingCommands = false; //This must be the last command, because route was successful.
                                routeTargetClient.RequestProcessor.ReceivingCommands = false;
                                routeTargetClient.RequestProcessor.OutputStream.WriteLine("ROUTED");
                                routeTargetClient.RequestProcessor.OutputStream.Flush();    
                                //var remoteClientEndpoint = (IPEndPoint)routeTargetClient.RequestProcessor._baseSocket.Client.RemoteEndPoint;
                                var socket1 = _baseSocket.Client;
                                var socket2 = routeTargetClient.RequestProcessor._baseSocket.Client;
                                Console.WriteLine("Creating route between {0} and {1} on request from {0}",
                                    HostClient.MemberId, routeTargetClient.MemberId);
                                routingController.CreateSocketRouteProxy(socket1, socket2);
                                routingController.CreateSocketRouteProxy(socket2, socket1);
                            }
                            else
                            {
                                OutputStream.WriteLine("FAIL");
                                OutputStream.Flush();
                            }
                            break;

                        case "keepalive":
                            _stayConnected = true;
                            break;

                        case "endcmds":
                            ReceivingCommands = false;
                            break;
                    }
                }
                //Got command, now keep alive
                _baseSocket.ReceiveTimeout = -1;
                while (_stayConnected)
                {
                    //Keep connection alive
                }
                Console.WriteLine($"Client {HostClient.MemberId} disconnected cleanly.");
            }
            catch (Exception ex)
            {
                if (_baseSocket.Connected)
                    _baseSocket.Close();
                Console.WriteLine($"Client {HostClient.MemberId} disconnected with an exception.");
            }
        }

        #endregion Public Methods

        #region Public Properties

        public RoutingConnectorClient HostClient { get; set; }
        public StreamReader InputStream { get; set; }
        public StreamWriter OutputStream { get; set; }
        public bool ReceivingCommands { get; set; } = true;

        #endregion Public Properties
    }
}