using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text;

namespace ChatClient
{
    internal static class Program
    {
        private static string UserName { get; set; }

        private static string Host { get; set; } = "127.0.0.1";

        private static int Port { get; } = 8888;

        private static TcpClient Client { get; set; }

        private static NetworkStream Stream { get; set; }

        internal static async Task Main(string[] args)
        {
            if (args.Length < 1) return;
            Host = args[0];
            
            Console.Write("Введите свое имя: ");
            UserName = Console.ReadLine();
            Client = new TcpClient();

            try
            {
                await Client.ConnectAsync(Host, Port);
                Stream = Client.GetStream();

                string message = UserName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                await Stream.WriteAsync(data, 0, data.Length);

                //Run new thread to get data
                RecieveMessage();
                Console.WriteLine($"Добро пожаловать, {UserName}!");
                await SendMessage();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        private static async ValueTask SendMessage()
        {
            Console.WriteLine("Введите сообщение: ");

            for (;;)
            {
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(message);
                await Stream.WriteAsync(data, 0, data.Length);
            }
        }

        private static async ValueTask RecieveMessage()
        {
            for (;;)
            {
                try
                {
                    byte[] data = new byte[64];
                    StringBuilder stringBuilder = new StringBuilder();
                    int bytes = 0;

                    do
                    {
                        bytes = await Stream.ReadAsync(data, 0, data.Length);
                        stringBuilder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (Stream.DataAvailable);

                    string message = stringBuilder.ToString();
                    Console.WriteLine(message);
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!");
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        private static void Disconnect()
        {
            if (Stream != null) Stream.Close();
            if (Client != null) Client.Close();
            Environment.Exit(0);
        }
    }
}
