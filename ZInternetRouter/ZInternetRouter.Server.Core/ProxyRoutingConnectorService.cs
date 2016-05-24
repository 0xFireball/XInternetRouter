using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ZInternetRouter.Server.Core
{
    /// <summary>
    ///     A class to manage instances of InternetRoutingProxy
    /// </summary>
    public class ProxyRoutingConnectorService
    {
        #region Public Constructors

        public ProxyRoutingConnectorService(string bindAddress, int bindPort)
        {
            _listenerSocket = new TcpListener(IPAddress.Parse(bindAddress), bindPort);
        }

        #endregion Public Constructors

        #region Private Fields

        private readonly TcpListener _listenerSocket;
        private bool _isActive;
        public List<RoutingConnectorClient> ConnectedClients { get; set; }

        #endregion Private Fields

        #region Public Methods

        public void StartRouter()
        {
            _listenerSocket.Start();
            ConnectedClients = new List<RoutingConnectorClient>();
            _isActive = true;
            while (_isActive)
            {
                var s = _listenerSocket.AcceptTcpClient();
                var client = new RoutingConnectorClient
                {
                    RequestProcessor = new RouteRequestProcessor(s, this)
                };
                client.RequestProcessor.HostClient = client;
                Task.Factory.StartNew(() => ProcessConnection(client));
            }
        }

        public void ProcessConnection(RoutingConnectorClient client)
        {
            ConnectedClients.Add(client);
            client.RequestProcessor.ProcessConnection();
            ConnectedClients.Remove(client);
        }

        #endregion Public Methods
    }
}