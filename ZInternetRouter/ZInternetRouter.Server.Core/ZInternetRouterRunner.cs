namespace ZInternetRouter.Server.Core
{
    internal class ZInternetRouterRunner
    {
        public static void RunServer(string bindAddress, int bindPort)
        {
            var routerServer = new ProxyRoutingConnectorService(bindAddress, bindPort);
            routerServer.StartRouter();
        }
    }
}