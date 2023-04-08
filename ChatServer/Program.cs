using System;
using System.Threading.Tasks;

namespace ChatServer
{
    internal static class Program
    {
        private static ServerObject Server { get; set; }

        internal static async Task Main()
        {
            try
            {
                Server = new ServerObject();
                await Server.Listen();
            }
            catch (Exception exception)
            {
                Server.Disconnect();
                Console.WriteLine(exception.Message);
            }
        }
    }
}
