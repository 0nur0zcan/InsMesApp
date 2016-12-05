﻿using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace InsMesAppServer
{
    class Program
    {
        public const int PORT_NO = 8765;
        public const string SERVER_IP = "127.0.0.1";

        private static TcpListener serverSocket;
        private static bool IsCommunicate;
        private static TcpClient clientSocket;
        private static Hashtable ClientList = new Hashtable();

        public static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse(SERVER_IP);
            serverSocket = new TcpListener(ip, PORT_NO);

            serverSocket.Start();
            Console.WriteLine("Anlık mesajlaşma sunucusu başladı....");
            Console.WriteLine("Sunucuyu durdurmak için 'ç' harfini giriniz....");
            IsCommunicate = true;
            Thread comThread = new Thread(Communicate);
            comThread.Start();

            string input = Console.ReadLine();
            if(input.Equals("ç"))
            {
                IsCommunicate = false;
                if (clientSocket != null)
                    clientSocket.Close();
                serverSocket.Stop();
                Console.WriteLine("Sunucu durdu...");
                Console.ReadLine();
            }
        }

        private static void Communicate()
        {
            while (IsCommunicate)
            {
                try
                {
                    clientSocket = serverSocket.AcceptTcpClient();

                    NetworkStream networkStream = clientSocket.GetStream();
                    BinaryReader reader = new BinaryReader(networkStream);

                    // Read an incoming message from a client
                    string usernameFromClient = reader.ReadString();

                    ClientList.Add(usernameFromClient, clientSocket);

                    // Notify all clients connected
                    Broadcast(usernameFromClient + " Katıldı", usernameFromClient, true);
                    Console.WriteLine(usernameFromClient + " Katıldı");

                    HandleClient client = new HandleClient();
                    client.Start(clientSocket, usernameFromClient);
                }
                catch
                {
                    // do nothing
                }
            }
        }

        public static void Broadcast(string msg, string userName, bool isUsername)
        {
            foreach (DictionaryEntry Item in ClientList)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)Item.Value;
                NetworkStream broadcastStream = broadcastSocket.GetStream();
                BinaryWriter writer = new BinaryWriter(broadcastStream);

                if (isUsername)
                {
                    writer.Write(msg);
                }
                else
                {
                    writer.Write(userName + ": " + msg);
                }

                writer.Flush();
            }
        }
    }

    public class HandleClient
    {
        private TcpClient ClientSocket;
        private string ClientName;

        public void Start(TcpClient inClientSocket, string clientName)
        {
            this.ClientSocket = inClientSocket;
            this.ClientName = clientName;

            Thread ctThread = new Thread(DoChat);
            ctThread.Start();
        }

        private void DoChat()
        {
            NetworkStream networkStream;
            BinaryReader reader;
            string dataFromClient;
            while (true)
            {
                try
                {
                    networkStream = ClientSocket.GetStream();
                    reader = new BinaryReader(networkStream);

                    // Read incoming message
                    dataFromClient = reader.ReadString();
                    Console.WriteLine(ClientName + ": " + dataFromClient);

                    Program.Broadcast(dataFromClient, ClientName, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}