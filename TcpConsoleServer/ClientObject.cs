#pragma warning disable CA1031 // Do not catch general exception types

using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpConsoleServer
{
    public sealed class ClientObject : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// 1Gb
        /// </summary>
        private const long DefaultBufferSize = 1073741824;

        private TcpClient Client { get; }

        public ClientObject(TcpClient tcpClient) => Client = tcpClient;

        public async ValueTask Process(string serverPath)
        {
            try
            {
                await using NetworkStream stream = Client.GetStream();

                while (true)
                {
                    //Memory buffer.
                    Memory<byte> buffer = new byte[64];
                    StringBuilder stringBuilder = new StringBuilder();

                    //Get file name from client
                    do
                    {
                        await stream.ReadAsync(buffer).ConfigureAwait(false);
                        stringBuilder.Append(Encoding.UTF8.GetString(buffer.Span));
                        //buffer = new byte[64];
                    }
                    while (stream.DataAvailable);

                    //Parse file name
                    string filePath = Path.Combine(serverPath, "11.png");
                    //string filePath = Path.Combine(serverPath, stringBuilder.ToString().Replace("\0", string.Empty).Trim());
                    FileInfo inputFileInfo = new FileInfo(filePath);
                    Console.WriteLine($"Client asked for {inputFileInfo.FullName}.");
                    if (!inputFileInfo.Exists) continue; //todo send message "no file"

                    //todo progress-reporting

                    long inputFileSize = inputFileInfo.Length;
                    string httpResponse = $"HTTP/1.1 200 OK\r\nContent-Length: {inputFileSize}\r\n\r\n";

                    //Send fileSize
                    buffer = Encoding.UTF8.GetBytes(httpResponse); //$"{inputFileSize}");
                    await stream.WriteAsync(buffer).ConfigureAwait(false);

                    Console.WriteLine("Долбежка в жопу!");

                    long bufferSize = inputFileSize < DefaultBufferSize ? inputFileSize : DefaultBufferSize;
                    buffer = new byte[bufferSize];
                    long bytesWritten = 0;

                    //double counter = 0.0;
                    //double diff = (double) bufferSize / inputFileInfo.Length;
                    await using FileStream inputFileStream = inputFileInfo.OpenRead();

                    //Write file to stream
                    while (await inputFileStream.ReadAsync(buffer).ConfigureAwait(false) > 0)
                    {
                        await stream.WriteAsync(buffer).ConfigureAwait(false);

                        bytesWritten += bufferSize;
                        long bytesLeft = inputFileSize - bytesWritten;
                        bufferSize = bytesLeft <= DefaultBufferSize ? bytesLeft : DefaultBufferSize;

                        buffer = new byte[bufferSize];

                        //counter++;
                        //var percentage = (bufferSize - inputFileInfo.Length) / inputFileInfo.Length
                    }
                }
            }
            catch (Exception exception) { Console.WriteLine(exception.Message); }
        }

        #region Dispose

        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsDisposed { get; private set; }

        ~ClientObject() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                //Occurs only if called by programmer. Dispose static things here.
            }

            Client?.Dispose();

            IsDisposed = true;
        }

        public ValueTask DisposeAsync()
        {
            try
            {
                Dispose();

                return default;
            }
            catch (Exception exception)
            {
                return new ValueTask(Task.FromException(exception));
            }
        }

        #endregion
    }
}

#pragma warning restore CA1031 // Do not catch general exception types
