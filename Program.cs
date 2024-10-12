using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MessageServer
{
    class Program
    {
        private static readonly Dictionary<string, List<string>> messageStorage = new Dictionary<string, List<string>>();

        static void Main()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine("Сервер запущен и ожидает подключений...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
        }

        private static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            string[] parts = receivedMessage.Split('|');
            string command = parts[0];

            if (command == "SEND")
            {
                string recipient = parts[1];
                string message = parts[2];
                StoreMessage(recipient, message);
                Console.WriteLine($"Сообщение для {recipient} сохранено.");
            }
            else if (command == "RECEIVE")
            {
                string recipient = parts[1];
                string messages = RetrieveMessages(recipient);
                byte[] response = Encoding.UTF8.GetBytes(messages);
                stream.Write(response, 0, response.Length);
            }

            client.Close();
        }

        private static void StoreMessage(string recipient, string message)
        {
            if (!messageStorage.ContainsKey(recipient))
            {
                messageStorage[recipient] = new List<string>();
            }
            messageStorage[recipient].Add(message);
        }

        private static string RetrieveMessages(string recipient)
        {
            if (messageStorage.ContainsKey(recipient) && messageStorage[recipient].Count > 0)
            {
                string allMessages = string.Join(Environment.NewLine, messageStorage[recipient]);
                messageStorage[recipient].Clear();
                return allMessages;
            }
            return "Нет новых сообщений.";
        }
    }
}
