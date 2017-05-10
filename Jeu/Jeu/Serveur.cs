using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using System.Windows.Forms;

namespace AtelierXNA
{
    // classe inspirée de vidéos au lien https://www.youtube.com/watch?v=CHba4o3wlgY&list=PL332FC684E7A3B3D4
    // réalisé par Nathan Biefeld
    // Grand merci à cet homme ...
    //évènements
    public delegate void ConnectionEvent(object sender, Client user);
    public delegate void DataReceivedEvent(Client sender, byte[] data);

    public class Client
    {

        private TcpClient client;


        private byte[] readBuffer;


        public event ConnectionEvent UserDisconnected;
        public event DataReceivedEvent DataReceived;


        public readonly byte id;

        public string IP;

        bool connected = false;

        public Client(TcpClient client, byte id)
        {
            readBuffer = new byte[2048];
            this.id = id;
            this.client = client;
            IP = client.Client.RemoteEndPoint.ToString();
            client.NoDelay = true;

            StartListening();
            connected = true;
        }

        public Client(string ip, int port)
        {
            readBuffer = new byte[2048];
            id = byte.MaxValue;
            client = new TcpClient();
            client.NoDelay = true;
            client.Connect(ip, port);

            StartListening();
            connected = true;
        }

        public void Disconnect()
        {
            if (connected)
            {
                connected = false;
                client.Close();

                if (UserDisconnected != null)
                    UserDisconnected(this, this);
            }
        }

        private void StartListening()
        {
            client.GetStream().BeginRead(readBuffer, 0, 2048, StreamReceived, null);
        }

        private void StreamReceived(IAsyncResult ar)
        {
            int bytesRead = 0;
            try
            {
                lock (client.GetStream())
                {
                    bytesRead = client.GetStream().EndRead(ar);
                }
            }

            catch (Exception e) { }

            if (bytesRead == 0)
            {
                Disconnect();
                Console.WriteLine("Client {0}:  {1}\n{2}", IP, "Bad data", "Disconnecting");
                return;
            }

            byte[] data = new byte[bytesRead];

            for (int i = 0; i < bytesRead; i++)
                data[i] = readBuffer[i];

            StartListening();

            if (DataReceived != null)
                DataReceived(this, data);
        }

        public void SendData(byte[] b)
        {
            try
            {
                lock (client.GetStream())
                {
                    client.GetStream().BeginWrite(b, 0, b.Length, null, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Client {0}:  {1}", IP, e.ToString());
            }
        }

        public void SendMemoryStream(System.IO.MemoryStream ms)
        {
            lock (ms)
            {
                int bytesWritten = (int)ms.Position;
                byte[] result = new byte[bytesWritten];

                ms.Position = 0;
                ms.Read(result, 0, bytesWritten);
                SendData(result);
            }
        }

        public override string ToString()
        {
            return IP;
        }
    }

    public class Listener
    {
        const int NB_MAX_DE_JOUEURS = 2;
        private TcpListener listener;

        public event ConnectionEvent userAdded;


        private bool[] usedUserID;

        public Listener(int portNr)
        {
            //Create an array to hold the used IDs
            usedUserID = new bool[2];

            //Create the internal TcpListener
            listener = new TcpListener(IPAddress.Any, portNr);
        }

        public void Start()
        {
            listener.Start();
            ListenForNewClient();
        }

        private void ListenForNewClient()
        {
            listener.BeginAcceptTcpClient(AcceptClient, null);
        }

        private void AcceptClient(IAsyncResult ar)
        {
            TcpClient client = listener.EndAcceptTcpClient(ar);

            int id = -1;
            for (byte i = 0; i < usedUserID.Length; i++)
            {
                if (usedUserID[i] == false)
                {
                    id = i;
                    break;
                }
            }

            if (id == -1)
            {
                Console.WriteLine("Client " + client.Client.RemoteEndPoint.ToString() + " cannot connect. ");
                return;
            }

            usedUserID[id] = true;
            Client newClient = new Client(client, (byte)id);

            newClient.UserDisconnected += new ConnectionEvent(client_UserDisconnected);

            if (userAdded != null)
                userAdded(this, newClient);

            ListenForNewClient();
        }

        void client_UserDisconnected(object sender, Client user)
        {
            usedUserID[user.id] = false;
        }

        public void Stop()
        {
            listener.Stop();
        }
    }

    public class Server
    {
        const int BUFFER_SIZE = 2048;
        public static Server singleton;

        Listener listener;
        TcpClient Client;
        byte[] ReadBuffer { get; set; }
        public Listener Listener
        {
            get { return listener; }
        }

        public Vector3 PositionJoueur { get; private set; }
        public Vector3 PositionEnemi { get; private set; }
        Client[] client;

        public int connectedClients { get; private set; } = 0;

        public bool enemyConnected { get; private set; }
        public bool EnnemiPrêtÀJouer { get; private set; }

        const int NB_MAX_DE_JOUEURS = 2;
        const int LIMITE = 100;
        public MemoryStream readStream;
        public MemoryStream writeStream;
        public BinaryReader reader;
        public BinaryWriter writer;


        public Server(int port, string ip)
        {

            //Initialize the array with a maximum of the MaxClients from the config file.
            client = new Client[NB_MAX_DE_JOUEURS];
            enemyConnected = false;
            //Create a new Listener object
            listener = new Listener(port);
            listener.userAdded += new ConnectionEvent(listener_userAdded);
            listener.Start();
            ReadBuffer = new byte[BUFFER_SIZE];
            //Create the readers and writers.
            readStream = new MemoryStream();
            writeStream = new MemoryStream();
            reader = new BinaryReader(readStream);
            writer = new BinaryWriter(writeStream);
            Client = new TcpClient();

            //Client.NoDelay = true;
            //Client.Connect(ip, port);
            //Set the singleton to the current object
            Server.singleton = this;

            //writeStream.Position = 0;
            //writer.Write((byte)Protocoles.Connected);
            //SendDataClient(GetDataFromMemoryStream(writeStream));
            //Client.GetStream().BeginRead(ReadBuffer, 0, BUFFER_SIZE, StreamReceived, null);
        }

        // pour pouvoir recommencer un serveur!
        public void Close()
        {
            singleton.Close();
            Client.Close();
            listener.Stop();

        }

        private void listener_userAdded(object sender, Client user)
        {
            connectedClients++;



            writeStream.Position = 0;
            byte nouveauUser = 1;
            writer.Write(nouveauUser);
            writer.Write(user.id);
            writer.Write(user.IP);

            SendData(GetDataFromMemoryStream(writeStream), user);
            user.DataReceived += new DataReceivedEvent(user_DataReceived);
            user.UserDisconnected += new ConnectionEvent(user_UserDisconnected);

            Console.WriteLine(user.ToString() + " connected\tConnected Clients:  " + connectedClients + "\n");

            client[user.id] = user;
        }
        //public void Envoyer(Protocoles protocole, bool val)
        //{
        //    writeStream.Position = 0;
        //    writer.Write((byte)protocole);
        //    writer.Write(val);
        //    SendDataClient(GetDataFromMemoryStream(writeStream));
        //}

        private void user_UserDisconnected(object sender, Client user)
        {
            connectedClients--;
            writeStream.Position = 0;
            writer.Write((byte)Protocoles.Disconnected);
            SendData(GetDataFromMemoryStream(writeStream));
            client[user.id] = null;
        }


        private void user_DataReceived(Client sender, byte[] data)
        {
            writeStream.Position = 0;
            SendData(data, sender);
        }

        // si jamais
        public void VérificationPositionServeur(Vector3 position)
        {
            float X = position.X;
            float Z = position.Z;

            X = X < LIMITE ? X : LIMITE;
            X = X > -LIMITE ? X : -LIMITE;
            Z = Z < LIMITE ? Z : LIMITE;
            Z = Z > -LIMITE ? Z : -LIMITE;

            PositionJoueur = new Vector3(X, position.Y, Z);
        }

        public byte[] GetDataFromMemoryStream(MemoryStream ms)
        {
            byte[] result;

            int bytesWritten = (int)ms.Position;
            result = new byte[bytesWritten];
            ms.Position = 0;
            ms.Read(result, 0, bytesWritten);

            return result;
        }

        private byte[] CombineData(byte[] data, MemoryStream ms)
        {
            byte[] result = GetDataFromMemoryStream(ms);

            byte[] combinedData = new byte[data.Length + result.Length];

            for (int i = 0; i < data.Length; i++)
            {
                combinedData[i] = data[i];
            }

            for (int j = data.Length; j < data.Length + result.Length; j++)
            {
                combinedData[j] = result[j - data.Length];
            }

            return combinedData;
        }

        public void SendData(byte[] data, Client sender)
        {
            foreach (Client c in client)
            {
                if (c != null && c != sender)
                {
                    c.SendData(data);
                }
            }

            writeStream.Position = 0;
        }

        private void SendData(byte[] data)
        {
            foreach (Client c in client)
            {
                if (c != null)
                    c.SendData(data);
            }

            writeStream.Position = 0;
        }

    }

}
