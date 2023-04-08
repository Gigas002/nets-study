using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ChatServer
{
    internal class ServerObject
    {
        private static TcpListener Listener { get; set; } //server to listen

        private List<ClientObject> Clients { get; set; } = new List<ClientObject>(); //all connections

        internal void AddConnection(ClientObject clientObject) => Clients.Add(clientObject);

        internal void RemoveConnection(string id)
        {
            //Get closed connection by id
            ClientObject clientObject = Clients.FirstOrDefault(client => client.Id == id);
            
            //Delete it from connections list
            if (clientObject != null) Clients.Remove(clientObject);
        }

        //Listen input messages
        internal async ValueTask Listen()
        {
            try
            {
                Listener = new TcpListener(IPAddress.Any, 8888);
                Listener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                for (;;)
                {
                    TcpClient tcpClient = await Listener.AcceptTcpClientAsync();
                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    clientObject.Process();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Disconnect();
            }
        }

        //Broadcast message to all connected clients
        internal async ValueTask BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);

            foreach (ClientObject client in Clients)
                if (client.Id != id)
                    await client.Stream.WriteAsync(data, 0, data.Length);
        }

        internal void Disconnect()
        {
            Listener.Stop(); //Stop the server
            
            foreach (ClientObject client in Clients) client.Close();

            Environment.Exit(0);
        }
    }
}