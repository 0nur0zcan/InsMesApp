using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace InsMesAppClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int PORT_NO = 8765;
        public const string IP = "127.0.0.1";

        private TcpClient ClientSocket = new TcpClient();
        private NetworkStream ServerStream;
        private bool IsCommunicate;
        private BinaryWriter Writer;
        private BinaryReader Reader;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnCon_Click(object sender, RoutedEventArgs e)
        {
            bool status = false;
            try
            {   // Chat Sunucusuna Bağlanılıyor...
                ClientSocket.Connect(IP, PORT_NO);
                status = true;
                BtnCon.IsEnabled = false;
                BtnSend.IsEnabled = true;
                TbxUsername.IsReadOnly = true;
            }
            catch
            {
                DisplayMessage("Chat Sunucusuna Bağlanılamadı!");
            }
            
            if(status)
            {   // Send username to the server
                ServerStream = ClientSocket.GetStream();
                Writer = new BinaryWriter(ServerStream);
                Writer.Write(TbxUsername.Text);
                Writer.Flush();

                // Start thread for receiving all msgs
                IsCommunicate = true;
                Thread receiverThread = new Thread(ReceiveMessage);
                receiverThread.Start();
            }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {   // Send message to the server
                Writer.Write(TbxMesaj.Text);
                Writer.Flush();

                TbxMesaj.Clear();
            }
            catch
            {   // Connection is terminated
                BtnSend.IsEnabled = false;
            }
        }

        private void ReceiveMessage()
        {
            while (IsCommunicate)
            {
                try
                {   // Read message
                    ServerStream = ClientSocket.GetStream();
                    Reader = new BinaryReader(ServerStream);

                    string dataReceived = Reader.ReadString();
                    DisplayMessage(dataReceived);
                }
                catch
                {   // do nothing
                }
            }
        }

        private void DisplayMessage(string dataReceived)
        {
            Dispatcher.Invoke(new Action(() => 
            {
                if (TbxMesajListesi.Text.Equals(""))
                    TbxMesajListesi.Text = ">> " + dataReceived;
                else
                    TbxMesajListesi.Text = TbxMesajListesi.Text + Environment.NewLine + ">> " + dataReceived;

                TbxMesajListesi.ScrollToEnd();
            }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsCommunicate = false;

            Writer.Close();
            if (Writer != null)
                Writer.Dispose();

            Reader.Close();
            if (Reader != null)
                Reader.Dispose();

            ServerStream.Close();
            if (ServerStream != null)
                ServerStream.Dispose();
        }
    }
}