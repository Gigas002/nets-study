using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace HttpFileServer
{
    internal static class Program
    {
        private static async ValueTask Listen()
        {
            using HttpListener httpListener = new HttpListener();

            Uri uri = new Uri("http://localhost:8888/connection/");

            //Установка адресов прослушки
            httpListener.Prefixes.Add(uri.AbsoluteUri);
            httpListener.Start();
            Console.WriteLine("Ожидание подключений...");

            while (true)
            {
                HttpListenerContext context = await httpListener.GetContextAsync().ConfigureAwait(false);

                // получаем объект ответа
                using HttpListenerResponse response = context.Response;

                var request = context.Request;

                // создаем ответ в виде кода html
                string responseStr = "<html><head><meta charset='utf8'></head><body>Привет мир!</body></html>";
                byte[] buffer = Encoding.UTF8.GetBytes(responseStr);

                // получаем поток ответа и пишем в него ответ
                response.ContentLength64 = buffer.Length;
                await using Stream output = response.OutputStream;
                await output.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                // закрываем поток
                output.Close();
            }

            // останавливаем прослушивание подключений
            httpListener.Stop();
        }

        static async Task Main()
        {
            await Listen().ConfigureAwait(false);



            Console.WriteLine("Обработка подключений завершена");
            Console.ReadKey();
        }
    }
}
