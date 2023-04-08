using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetsTests
{
    public static class SocketTcpServer
    {
        public static int Port { get; } = 8005;

        public static async ValueTask RunServer()
        {
            //Get sockets addresses
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);

            //Create socket
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                //Bind socket with local point, that'll get data
                listenSocket.Bind(iPEndPoint);

                //Start listen
                listenSocket.Listen(10);

                Console.WriteLine("Server is running. Waiting for connections...");

                while (true)
                {
                    Socket socket = await listenSocket.AcceptAsync();
                    //Get message
                    StringBuilder stringBuilder = new StringBuilder();
                    int bytes = 0; //got count of bytes
                    byte[] data = new byte[256]; //buffer

                    do
                    {
                        bytes = socket.Receive(data); //todo async
                        stringBuilder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (socket.Available > 0);

                    Console.WriteLine($"{DateTime.Now.ToShortTimeString()}, {stringBuilder.ToString()}");

                    //Recieve answer
                    string message = "Message got!";
                    data = Encoding.Unicode.GetBytes(message);
                    socket.Send(data); //todo async

                    //Close socket
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

        }
    }
}