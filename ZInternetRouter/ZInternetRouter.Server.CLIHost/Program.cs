using ZInternetRouter.Server.Core;

namespace ZInternetRouter.Server.CLIHost
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                ZInternetRouterRunner.RunServer(args[0], int.Parse(args[1]));
            }
            else
            {
                ZInternetRouterRunner.RunServer("0.0.0.0", 35121);
            }
        }
    }
}