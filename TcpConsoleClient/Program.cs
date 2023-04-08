using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpConsoleClient
{
    internal static class Program
    {
        /// <summary>
        /// 1Gb
        /// </summary>
        private const long DefaultBufferSize = 1073741824;

        private static int Port { get; } = 8888;

        private static string Address { get; } = "127.0.0.1";

        private static IPEndPoint GetIpEndPoint()
        {
            ReadOnlySpan<char> addressSpan = $"{Address}:{Port}";
            return IPEndPoint.Parse(addressSpan);
        }

        private static async Task Main()
        {
            DirectoryInfo downloadDirectoryInfo = new DirectoryInfo("D:/Client");
            downloadDirectoryInfo.Create();
            downloadDirectoryInfo.Refresh();

            //IPEndPoint ipEndPoint = GetIpEndPoint();
            //Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //listenSocket.Bind(ipEndPoint);

            try
            {
                using TcpClient tcpClient = new TcpClient(Address, Port);
                //tcpClient = new TcpClient(GetIpEndPoint());
                await using NetworkStream stream = tcpClient.GetStream();

                while (true)
                {
                    Console.Write("Input desired file's name: ");
                    string fileName = Console.ReadLine();

                    //Convert string to byte array
                    Memory<byte> buffer = Encoding.UTF8.GetBytes(fileName);
                    await stream.WriteAsync(buffer).ConfigureAwait(false);

                    //Get filesize
                    StringBuilder stringBuilder = new StringBuilder();
                    buffer = new byte[64];
                    do
                    {
                        await stream.ReadAsync(buffer).ConfigureAwait(false);
                        stringBuilder.Append(Encoding.UTF8.GetString(buffer.Span));
                    }
                    while (stream.DataAvailable);

                    //Set buffer size
                    string outputFileSizeString = stringBuilder.ToString().Replace("\0", string.Empty).Trim();
                    long outputFileSize = long.Parse(outputFileSizeString);
                    //long outputFileSize = 2818916797;
                    long bufferSize = outputFileSize < DefaultBufferSize ? outputFileSize : DefaultBufferSize;
                    buffer = new byte[bufferSize];
                    long bytesRead = 0;

                    FileInfo outputFileInfo = new FileInfo(Path.Combine(downloadDirectoryInfo.FullName, fileName));
                    //Get file response
                    await using FileStream outputFileStream = outputFileInfo.OpenWrite();
                    do
                    {
                        await stream.ReadAsync(buffer).ConfigureAwait(false);
                        await outputFileStream.WriteAsync(buffer).ConfigureAwait(false);

                        bytesRead += bufferSize;
                        long bytesLeft = outputFileSize - bytesRead;
                        bufferSize = bytesLeft <= DefaultBufferSize ? bytesLeft : DefaultBufferSize;

                        buffer = new byte[bufferSize];
                    }
                    while (stream.DataAvailable);

                    //Print message
                    //message = stringBuilder.ToString();
                    //Console.WriteLine($"Server: {message}");
                    Console.WriteLine("Download complete!");
                }
            }
            catch (Exception exception) { Console.WriteLine(exception.Message); }
        }
    }
}
