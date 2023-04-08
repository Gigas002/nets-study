using System;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatServer
{
    internal class ClientObject
    {
        internal string Id { get; private set; }

        internal NetworkStream Stream { get; private set; }

        private string UserName { get; set; }

        private TcpClient Client { get; set; }

        private ServerObject Server { get; set; }

        internal ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            Client = tcpClient;
            Server = serverObject;
            serverObject.AddConnection(this);                        
        }

        internal async ValueTask Process()
        {
            try
            {
                Stream = Client.GetStream();

                //Get user name
                string message = await GetMessage();
                UserName = message;
                
                message = $"{UserName} вошел в чат.";

                //Send this message to all connected clients
                await Server.BroadcastMessage(message, Id);
                Console.WriteLine(message);

                //Get message in infinite loop
                for (;;)
                {
                    try
                    {
                        message = await GetMessage();
                        message = $"{UserName}: {message}";
                        Console.WriteLine(message);
                        await Server.BroadcastMessage(message, Id);
                    }
                    catch
                    {
                        message = $"{UserName}: покинул чат.";
                        Console.WriteLine(message);
                        await Server.BroadcastMessage(message, Id);
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                //In case loop ended - dispose resources
                Server.RemoveConnection(Id);
                Close();
            }
        }

        //Read input message and convert to string
        private async ValueTask<string> GetMessage()
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

            return stringBuilder.ToString();
        }

        //Close the connection
        internal void Close()
        {
            if (Stream != null) Stream.Close();
            if (Client != null) Client.Close();
        }
    }
}