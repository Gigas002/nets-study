using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetsTests
{
    public static class SocketTcpClient
    {
        //Port to connect
        public static int Port { get; } = 8005;
        
        //Address
        public static string Address { get; } = "127.0.0.1";

        public static async ValueTask RunClient()
        {
            try
            {
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(Address), Port);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Connect the host
                await socket.ConnectAsync(iPEndPoint);
                Console.Write("Write a message:");
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(message);
                socket.Send(data);

                //Get response
                data = new byte[256]; //buffer
                StringBuilder stringBuilder = new StringBuilder();
                int bytes = 0; //bytes count

                do
                {
                    bytes = socket.Receive(data, data.Length, 0); //todo async
                    stringBuilder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (socket.Available > 0);
                Console.WriteLine($"Server answered: {stringBuilder.ToString()}");

                //Close the socket
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            Console.Read();
        }    
    }
}