using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace UdpClient
{
    internal static class Program
    {
        //Порт приема сообщений
        private static int LocalPort { get; set; }

        //Порт для отправки сообщений
        private static int RemotePort { get; set; }

        private static Socket ListeningSocket { get; set; }

        private static string LocalAddress { get; } = "127.0.0.1";

        private static int BytesToListen { get; } = 256;

        internal static async Task Main(string[] args)
        {
            Console.Write("Введите порт для приема сообщений: ");
            LocalPort = int.Parse(Console.ReadLine());
            Console.Write("Введите порт для отправки сообщений: ");
            RemotePort = int.Parse(Console.ReadLine());
            Console.WriteLine("Для отправки сообщений введите сообщение и нажмите Enter.");
            Console.WriteLine();

            try
            {
                ListeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                //Прослушиваем
                Task.Run(() => Listen());

                //Отправка сообщений на разные порты
                await Task.Run(() => Send());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                Close();
            }
        }

        private static void Send()
        {
            for(;;)
            {
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(message);
                EndPoint remotePoint = new IPEndPoint(IPAddress.Parse(LocalAddress), RemotePort);
                ListeningSocket.SendTo(data, remotePoint); //todo async
            }
        }

        private static void Listen()
        {
            //Прослушиваем по адресу
            IPEndPoint localIP = new IPEndPoint(IPAddress.Parse(LocalAddress), LocalPort);
            ListeningSocket.Bind(localIP);

            for (;;)
            {
                if (ListeningSocket.Available <= 0) continue;

                //Получаем сообщение
                StringBuilder stringBuilder = new StringBuilder();
                int bytes = 0; //Количество полученных байтов.
                byte[] data = new byte[BytesToListen]; //Буфер для полученных данных.

                //Адрес, с которого пришли данные
                EndPoint remoteIP = new IPEndPoint(IPAddress.Any, 0);

                for (;ListeningSocket.Available > 0;)
                {
                    bytes = ListeningSocket.ReceiveFrom(data, ref remoteIP); //todo async
                        // ListeningSocket.ReceiveFromAsync(new SocketAsyncEventArgs(){
                        //     RemoteEndPoint = remoteIP,
                        //     // Buffer = data
                        // });
                    stringBuilder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }

                //Получаем данные о подключении
                IPEndPoint remoteFullIP = remoteIP as IPEndPoint;

                //Выводим сообщение
                Console.WriteLine($"{remoteFullIP.Address}:{remoteFullIP.Port} - {stringBuilder}");
            }
        }

        //Закрытие сокета.
        private static void Close()
        {
            if (ListeningSocket != null)
            {
                ListeningSocket.Shutdown(SocketShutdown.Both);
                ListeningSocket.Close();
                ListeningSocket = null;
                ListeningSocket.Dispose();
            }
        }
    }
}
