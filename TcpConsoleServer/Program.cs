#pragma warning disable CA1031 // Do not catch general exception types

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpConsoleServer
{
    internal static class Program
    {
        private static int Port { get; } = 8888;
        private static string Address { get; } = "127.0.0.1";

        private static async Task Main()
        {
            const string fileServerLocation = "D:/Server";

            TcpListener tcpListener = null;

            try
            {
                tcpListener = new TcpListener(IPAddress.Parse(Address), Port);
                tcpListener.Start();
                Console.WriteLine("Waiting for connections...");

                while (true)
                {
                    using TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    await using ClientObject clientObject = new ClientObject(tcpClient);

                    //Run in new thread
                    await clientObject.Process(fileServerLocation).ConfigureAwait(false);
                }
            }
            catch (Exception exception) { Console.WriteLine(exception.Message); }
            finally
            {
                tcpListener?.Stop();
            }
        }

#pragma warning disable IDE0051 // Remove unused private members
        // ReSharper disable once UnusedMember.Local
        private static IPEndPoint GetIpEndPoint()
        {
            ReadOnlySpan<char> addressSpan = $"{Address}:{Port}";
            return IPEndPoint.Parse(addressSpan);
        }
#pragma warning restore IDE0051 // Remove unused private members
    }
}
#pragma warning restore CA1031 // Do not catch general exception types
