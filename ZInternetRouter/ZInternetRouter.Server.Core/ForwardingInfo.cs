using System.Net.Sockets;

namespace ZInternetRouter.Server.Core
{
    internal class ForwardingInfo
    {
        #region Public Methods

        public ForwardingInfo(Socket source, Socket destination)
        {
            SourceSocket = source;
            DestinationSocket = destination;
            Buffer = new byte[8192];
        }

        #endregion Public Methods

        #region Public Properties

        public byte[] Buffer { get; private set; }
        public Socket DestinationSocket { get; private set; }
        public Socket SourceSocket { get; private set; }
        public bool ThrowOnError = true;

        #endregion Public Properties
    }
}