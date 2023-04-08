using System;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HttpConsoleServer
{
    internal static class Program
    {
        #region Properties

        private static int DefaultBufferSize { get; } = 8192;
        private static string Address { get; set; } = "127.0.0.1";

        private static int Port { get; } = 8888;

        #endregion

        private static async Task Main(string[] args)
        {
            string serverLocation = args.Length > 0 ? args[0] : "E:/DataTests";
            Uri uri = new Uri($"http://{Address}:{Port}");

            using HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add(uri.AbsoluteUri);
            httpListener.Start();
            Console.WriteLine("Waiting for connections...");

            while (true)
                if (!await RunServer(httpListener, serverLocation))
                    break;

            httpListener.Stop();

            Console.WriteLine("Server closed with errors.");
            Console.ReadKey();
        }

        private static async ValueTask<bool> RunServer(HttpListener httpListener, string serverLocation)
        {
            try
            {
                HttpListenerContext httpListenerContext = await httpListener.GetContextAsync();

                string requestedPath = GetRequestedPath(httpListenerContext);
                string filePath = Path.Combine(serverLocation, requestedPath.Remove(0, 1));

                #if DEBUG
                Console.WriteLine($"Client requested {filePath}");
                #endif

                //TODO: catch inner exceptions
                Task.Run(() => WriteResponse(httpListenerContext, filePath));

                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                if (!httpListener.IsListening) return false;

                Task.Run(() => TrySendErrorMessage(httpListener, exception.Message));

                return true;
            }
        }

        private static async ValueTask TrySendErrorMessage(HttpListener httpListener, string errorMessage = "Error!")
        {
            try
            {
                HttpListenerContext httpListenerContext = await httpListener.GetContextAsync();

                using HttpListenerResponse httpListenerResponse = httpListenerContext.Response;

                //string htmlString = $"<html><head><meta charset='utf8'></head><body>{errorMessage}</body></html>";
                GetResponse(httpListenerResponse, errorMessage.Length);

                Memory<byte> buffer = Encoding.UTF8.GetBytes(errorMessage);

                await using Stream outputStream = httpListenerResponse.OutputStream;
                await outputStream.WriteAsync(buffer);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private static string GetRequestedPath(HttpListenerContext httpListenerContext)
        {
            HttpListenerRequest httpListenerRequest = httpListenerContext.Request;
            return httpListenerRequest.RawUrl;
        }

        private static void GetResponse(HttpListenerResponse httpListenerResponse, long contentLength)
        {
            //Set needed headers n' stuff here
            httpListenerResponse.ContentLength64 = contentLength;
            //httpListenerResponse.ContentType = "image/tiff";
        }

        private static async ValueTask WriteResponse(HttpListenerContext httpListenerContext, string filePath)
        {
            using HttpListenerResponse httpListenerResponse = httpListenerContext.Response;

            FileInfo fileInfo = new FileInfo(filePath);
            await using FileStream fileStream = fileInfo.OpenRead();
            long fileSize = fileInfo.Length;
            GetResponse(httpListenerResponse, fileSize);

            long bufferSize = fileSize < DefaultBufferSize ? fileSize : DefaultBufferSize;
            Memory<byte> buffer = new byte[bufferSize];
            long bytesWritten = 0;

            await using Stream outputStream = httpListenerResponse.OutputStream;
            while (await fileStream.ReadAsync(buffer) > 0)
            {
                await outputStream.WriteAsync(buffer);

                bytesWritten += bufferSize;
                long bytesLeft = fileSize - bytesWritten;
                bufferSize = bytesLeft <= DefaultBufferSize ? bytesLeft : DefaultBufferSize;

                buffer = new byte[bufferSize];
            }
        }
    }
}
