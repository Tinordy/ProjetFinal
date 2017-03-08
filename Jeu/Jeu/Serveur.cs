using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;

namespace AtelierXNA
{
    // delegates!! :) fallait pas oublier
    public delegate void ConnectionEvent(object sender, Client user);
    public delegate void DataReceivedEvent(Client sender, byte[] data);

    public class Client
    {
        //Encapsulated 
        private TcpClient client;

        //Byte array that is populated when a user receives data
        private byte[] readBuffer;

        //Create the events
        public event ConnectionEvent UserDisconnected;
        public event DataReceivedEvent DataReceived;

        //The ID of this client, the constructor is only allowed to set this variable
        public readonly byte id;

        //IP of the connected client
        public string IP;

        //Is this client disconnected?
        bool connected = false;

        /// <summary>
        /// Create a new client
        /// </summary>
        /// <param name="client">TcpClient object to use</param>
        /// <param name="id">ID to give to the client</param>
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

        /// <summary>
        /// Create an empty Client object
        /// </summary>
        /// <param name="ip">IP to give to the client</param>
        /// <param name="port">Port to connect</param>
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

        /// <summary>
        /// Disconnect the client from the server
        /// </summary>
        public void Disconnect()
        {
            if (connected)
            {
                connected = false;
                client.Close();

                //Call all delegates
                if (UserDisconnected != null)
                    UserDisconnected(this, this);
            }
        }

        /// <summary>
        /// Start listening for new data
        /// </summary>
        private void StartListening()
        {
            client.GetStream().BeginRead(readBuffer, 0, 2048, StreamReceived, null);
        }

        /// <summary>
        /// Data was received
        /// </summary>
        /// <param name="ar">Async status</param>
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

            //An error happened that created bad data
            if (bytesRead == 0)
            {
                Disconnect();
                Console.WriteLine("Client {0}:  {1}\n{2}", IP, "Bad data", "Disconnecting");
                return;
            }

            //Create the byte array with the number of bytes read
            byte[] data = new byte[bytesRead];

            //Populate the array
            for (int i = 0; i < bytesRead; i++)
                data[i] = readBuffer[i];

            //Listen for new data
            StartListening();

            //Call all delegates
            if (DataReceived != null)
                DataReceived(this, data);
        }

        /// <summary>
        /// Code to actually send the data to the client
        /// </summary>
        /// <param name="b">Data to send</param>
        public void SendData(byte[] b)
        {
            //Try to send the data.  If an exception is thrown, disconnect the client
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

        /// <summary>
        /// Code to send data in a MemoryStream format
        /// </summary>
        /// <param name="ms">The data to send</param>
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

        /// <summary>
        /// String representation of the Client
        /// </summary>
        /// <returns>IP address</returns>
        public override string ToString()
        {
            return IP;
        }
    }

    public class Listener
    {
        const int NB_MAX_DE_JOUEURS = 2;
        private TcpListener listener;

        //send an even once we receive a user
        public event ConnectionEvent userAdded;


        //a variable to keep track of how many users we've added
        private bool[] usedUserID;

        /// <summary>
        /// Create a new Listener object
        /// </summary>
        /// <param name="portNr">Port to use</param>
        public Listener(int portNr)
        {
            //Create an array to hold the used IDs
            usedUserID = new bool[2];

            //Create the internal TcpListener
            listener = new TcpListener(IPAddress.Any, portNr);
        }

        /// <summary>
        /// Starts a new session of listening for messages.
        /// </summary>
        public void Start()
        {
            listener.Start();
            ListenForNewClient();
        }

        /// <summary>
        /// Stops listening for messages.
        /// </summary>
        public void Stop()
        {
            listener.Stop();
        }

        /// <summary>
        /// Used for allowing new users to connect
        /// </summary>
        private void ListenForNewClient()
        {
            listener.BeginAcceptTcpClient(AcceptClient, null);
        }

        /// <summary>
        /// Called when a client connects to the server
        /// </summary>
        /// <param name="ar">Status of the Async method</param>
        private void AcceptClient(IAsyncResult ar)
        {
            //We need to end the Async method of accepting new clients
            TcpClient client = listener.EndAcceptTcpClient(ar);

            //id is originally -1 which means a user cannot connect
            int id = -1;
            for (byte i = 0; i < usedUserID.Length; i++)
            {
                if (usedUserID[i] == false)
                {
                    id = i;
                    break;
                }
            }

            //If the id is still -1, the client what wants to connect cannot (probably because we have reached the maximum number of clients
            if (id == -1)
            {
                Console.WriteLine("Client " + client.Client.RemoteEndPoint.ToString() + " cannot connect. ");
                return;
            }

            //ID is valid, so create a new Client object with the server ID and IP
            usedUserID[id] = true;
            Client newClient = new Client(client, (byte)id);

            //We are now connected, so we need to set up the User Disconnected event for this user.
            newClient.UserDisconnected += new ConnectionEvent(client_UserDisconnected);

            //We are now connected, so call all delegates of the UserAdded event.
            if (userAdded != null)
                userAdded(this, newClient);

            //Begin listening for new clients
            ListenForNewClient();
        }

        /// <summary>
        /// User disconnects from the server
        /// </summary>
        /// <param name="sender">Original object that called this method</param>
        /// <param name="user">Client to disconnect</param>
        void client_UserDisconnected(object sender, Client user)
        {
            usedUserID[user.id] = false;
        }

    }

    public class Server
    {
        //Singleton in case we need to access this object without a reference (call <Class_Name>.singleton)
        public static Server singleton;

        //Create an object of the Listener class.
        Listener listener;
        public Listener Listener
        {
            get { return listener; }
        }

        //Array of clients
        Client[] client;

        //number of connected clients
        public int connectedClients { get; private set; } = 0;
        const int NB_MAX_DE_JOUEURS = 2;
        const int LIMITE = 100;
        //Writers and readers to manipulate data
        MemoryStream readStream;
        MemoryStream writeStream;
        BinaryReader reader;
        BinaryWriter writer;

        /// <summary>
        /// Create a new Server object
        /// </summary>
        /// <param name="port">The port you want to use</param>
        public Server(int port)
        {
            
            //Initialize the array with a maximum of the MaxClients from the config file.
            client = new Client[NB_MAX_DE_JOUEURS];

            //Create a new Listener object
            listener = new Listener(port);
            listener.userAdded += new ConnectionEvent(listener_userAdded);
            listener.Start();

            //Create the readers and writers.
            readStream = new MemoryStream();
            writeStream = new MemoryStream();
            reader = new BinaryReader(readStream);
            writer = new BinaryWriter(writeStream);

            //Set the singleton to the current object
            Server.singleton = this;
        }

        /// <summary>
        /// Method that is performed when a new user is added.
        /// </summary>
        /// <param name="sender">The object that sent this message</param>
        /// <param name="user">The user that needs to be added</param>
        private void listener_userAdded(object sender, Client user)
        {
            connectedClients++;


            writeStream.Position = 0;
            byte nouveauUser = 1;
            writer.Write(nouveauUser);
            writer.Write(user.id);
            writer.Write(user.IP);

            SendData(GetDataFromMemoryStream(writeStream), user);

            //Set up the events
            user.DataReceived += new DataReceivedEvent(user_DataReceived);
            user.UserDisconnected += new ConnectionEvent(user_UserDisconnected);

            //Print the new player message to the server window.
            Console.WriteLine(user.ToString() + " connected\tConnected Clients:  " + connectedClients + "\n");

            //Add to the client array
            client[user.id] = user;
        }

        /// <summary>
        /// Method that is performed when a new user is disconnected.
        /// </summary>
        /// <param name="sender">The object that sent this message</param>
        /// <param name="user">The user that needs to be disconnected</param>
        private void user_UserDisconnected(object sender, Client user)
        {
            connectedClients--;

            //Print the removed player message to the server window.
            Console.WriteLine(user.ToString() + " disconnected\tConnected Clients:  " + connectedClients + "\n");

            //Clear the array's index
            client[user.id] = null;
        }

        /// <summary>
        /// Relay messages sent from one client and send them to others
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="data">The data to relay</param>
        private void user_DataReceived(Client sender, byte[] data)
        {
            writeStream.Position = 0;
            SendData(data, sender);
        }
        
        public Vector3 VérificationPositionServeur(Vector3 position)
        {
            float X = position.X;
            float Z = position.Z;

            X = X < LIMITE ? X : LIMITE;
            X = X > -LIMITE ? X : -LIMITE;          
            Z = Z < LIMITE ? Z : LIMITE;
            Z = Z > -LIMITE ? Z : -LIMITE;


            return new Vector3(X, position.Y, Z);
        }

        /// <summary>
        /// Converts a MemoryStream to a byte array
        /// </summary>
        /// <param name="ms">MemoryStream to convert</param>
        /// <returns>Byte array representation of the data</returns>
        public byte[] GetDataFromMemoryStream(MemoryStream ms)
        {
            byte[] result;
            float limite = 10f;

            float X;
            float Y;
            float Z;
            int bytesWritten = (int)ms.Position;
            result = new byte[bytesWritten];
            ms.Position = 0;
           
            //if ((int)ms.Position > 0)
            //{
            //    Protocoles p = (Protocoles)reader.ReadByte();
            //    if (p == Protocoles.PlayerMoved)
            //    {
            //        X = reader.ReadSingle();
            //        Y = reader.ReadSingle();
            //        Z = reader.ReadSingle();

            //        Vector3 tampon =  VérificationPositionServeur(new Vector3(X, Y, Z));

            //        // réécrire le memory Stream

            //        writeStream.Position = 0;

            //        writer.Write((byte)Protocoles.PlayerMoved);
            //        writer.Write(tampon.X);
            //        writer.Write(tampon.Y);
            //        writer.Write(tampon.Z);
            //        writeStream.Read(result, 0, bytesWritten);
            //        return result;
            //    }
            //}
            ms.Read(result, 0, bytesWritten);

            return result;
        }

        /// <summary>
        /// Combines one byte array with a MemoryStream
        /// </summary>
        /// <param name="data">Original Message in byte array format</param>
        /// <param name="ms">Message to append to the original message in MemoryStream format</param>
        /// <returns>Combined data in byte array format</returns>
        private byte[] CombineData(byte[] data, MemoryStream ms)
        {
            //Get the byte array from the MemoryStream
            byte[] result = GetDataFromMemoryStream(ms);

            //Create a new array with a size that fits both arrays
            byte[] combinedData = new byte[data.Length + result.Length];

            //Add the original array at the start of the new array
            for (int i = 0; i < data.Length; i++)
            {
                combinedData[i] = data[i];
            }

            //Append the new message at the end of the new array
            for (int j = data.Length; j < data.Length + result.Length; j++)
            {
                combinedData[j] = result[j - data.Length];
            }

            //Return the combined data
            return combinedData;
        }

        /// <summary>
        /// Sends a message to every client except the source.
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <param name="sender">Client that should not receive the message</param>
        private void SendData(byte[] data, Client sender)
        {
            foreach (Client c in client)
            {
                if (c != null && c != sender)
                {
                    c.SendData(data);
                }
            }

            //Reset the writestream's position
            writeStream.Position = 0;
        }

        /// <summary>
        /// Sends data to all clients
        /// </summary>
        /// <param name="data">Data to send</param>
        private void SendData(byte[] data)
        {
            foreach (Client c in client)
            {
                if (c != null)
                    c.SendData(data);
            }

            //Reset the writestream's position
            writeStream.Position = 0;
        }
    }

}
