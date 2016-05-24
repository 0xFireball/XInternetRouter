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

        public bool ReceivingCommands { get; set; } = true;

        #region Public Properties

        public RoutingConnectorClient HostClient { get; set; }

        #endregion Public Properties

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
            try
            {
                var inputStream = new StreamReader(_baseSocket.GetStream());
                var outputStream = new StreamWriter(new BufferedStream(_baseSocket.GetStream()));
                outputStream.WriteLine(HostClient.MemberId);
                outputStream.Flush();
                //Impatiently wait for data
                //_baseSocket.ReceiveTimeout = 10000;
                while (ReceivingCommands)
                {
                    var command = inputStream.ReadLine();
                    switch (command)
                    {
                        case "setid":
                            var id = inputStream.ReadLine();
                            HostClient.MemberId = id;
                            break;

                        case "getroute":
                            var targetRouteId = inputStream.ReadLine();
                            var targetCandidates =
                                _proxyRoutingConnectorService.ConnectedClients.Where(cc => cc.MemberId == targetRouteId)
                                    .ToList();
                            if (targetCandidates.Count > 0)
                            {
                                outputStream.WriteLine("SUCCESS");
                                outputStream.Flush();
                                var routingController = new SocketRoutingService();
                                var routeTargetClient = targetCandidates[0];
                                ReceivingCommands = false;
                                routeTargetClient.RequestProcessor.ReceivingCommands = false;
                                //var remoteClientEndpoint = (IPEndPoint)routeTargetClient.RequestProcessor._baseSocket.Client.RemoteEndPoint;
                                var socket1 = _baseSocket.Client;
                                var socket2 = routeTargetClient.RequestProcessor._baseSocket.Client;
                                routingController.CreateSocketRouteProxy(socket1, socket2);
                                routingController.CreateSocketRouteProxy(socket2, socket1);
                            }
                            else
                            {
                                outputStream.WriteLine("FAIL");
                                outputStream.Flush();
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
            }
            catch (Exception ex)
            {
                if (_baseSocket.Connected)
                    _baseSocket.Close();
            }
        }

        #endregion Public Methods
    }
}