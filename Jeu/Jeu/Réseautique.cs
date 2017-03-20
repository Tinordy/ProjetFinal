using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace AtelierXNA
{
    class Réseautique
    {
        const int BUFFER_SIZE = 2048;
        Server Serveur { get; set; }
        TcpClient Client { get; set; }
        byte[] ReadBuffer { get; set; }
        public bool enemiConnecté { get; private set; }
        public MemoryStream readStream, writeStream;

        public BinaryReader reader;
        public BinaryWriter writer;
        public bool EnnemiPrêtÀJouer { get; private set; }
        public Réseautique(Server serveur, string ip, int port)
        {
            Serveur = serveur;
            EnnemiPrêtÀJouer = false;

            Client = new TcpClient();
            readStream = new MemoryStream();
            writeStream = new MemoryStream();

            enemiConnecté = false;

            reader = new BinaryReader(readStream);
            writer = new BinaryWriter(writeStream);
            Client.NoDelay = true;
            Client.Connect(ip, port);

            ReadBuffer = new byte[BUFFER_SIZE];

            writeStream.Position = 0;
            writer.Write((byte)Protocoles.Connected);
            SendData(Serveur.GetDataFromMemoryStream(writeStream));
            Client.GetStream().BeginRead(ReadBuffer, 0, BUFFER_SIZE, StreamReceived, null);
        }
        void StreamReceived(IAsyncResult ar)
        {
            int bytesRead = 0;

            try
            {
                lock (Client.GetStream())
                {
                    bytesRead = Client.GetStream().EndRead(ar);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "1");
            }

            if (bytesRead == 0)
            {
                Client.Close();
                return;
            }

            byte[] data = new byte[bytesRead];

            for (int cpt = 0; cpt < bytesRead; cpt++)
                data[cpt] = ReadBuffer[cpt];

            ProcessData(data);


            Client.GetStream().BeginRead(ReadBuffer, 0, BUFFER_SIZE, StreamReceived, null);
        }

        private void ProcessData(byte[] data)
        {
            readStream.SetLength(0);
            readStream.Position = 0;
            readStream.Write(data, 0, data.Length);
            readStream.Position = 0;

            Protocoles p;

            //try
            //{
            p = (Protocoles)reader.ReadByte();

            if (p == Protocoles.Connected)
            {
                if (!enemiConnecté)
                {
                    enemiConnecté = true;
                    //float X = reader.ReadSingle();
                    //float Y = reader.ReadSingle();
                    //float Z = reader.ReadSingle();
                    //enemy.Position = new Vector3(X, Y, Z);
                    //enemy = new Maison(this, 1f, Vector3.Zero, new Vector3(X,Y,Z), new Vector3(5f, 5f, 5f), "PlayerPaper", "EnemyPaper", INTERVALLE_MAJ_STANDARD);
                    //Components.Add(enemy);


                    writeStream.Position = 0;
                    writer.Write((byte)Protocoles.Connected);
                    SendData(Serveur.GetDataFromMemoryStream(writeStream));
                }

            }
            else if (p == Protocoles.Disconnected)
            {
                enemiConnecté = false;
            }
            else if (p == Protocoles.PlayerMoved)
            {
                float X = reader.ReadSingle();
                float Y = reader.ReadSingle();
                float Z = reader.ReadSingle();

                //faire une fonction!
               //Ennemi.Position = new Vector3(X, Y, Z);
            }
            else if (p == Protocoles.PositionInitiale)
            {
                float X = reader.ReadSingle();
                float Y = reader.ReadSingle();
                float Z = reader.ReadSingle();

                //Ennemi = new Maison(Game, 1f, Vector3.Zero, new Vector3(X, Y, Z), new Vector3(2, 2, 2), "brique1", "roof", 0.01f);
                //voiture mtn...

                //writeStream.Position = 0;
                //writer.Write((byte)Protocoles.PlayerMoved);
                //writer.Write(Joueur.Position.X);
                //writer.Write(Joueur.Position.Y);
                //writer.Write(Joueur.Position.Z);
                //SendData(Serveur.GetDataFromMemoryStream(writeStream));
            }
            else if (p == Protocoles.ReadyToPlayChanged)
            {
                EnnemiPrêtÀJouer = reader.ReadBoolean();
            }
        }
        public void SendData(byte[] b)
        {
            try
            {
                lock (Client.GetStream())
                {
                    Client.GetStream().BeginWrite(b, 0, b.Length, null, null);
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
