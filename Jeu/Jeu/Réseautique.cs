using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace AtelierXNA
{
    public class Réseautique : Microsoft.Xna.Framework.GameComponent
    {
        public TimerAugmente TempsDeCourseJ { get; set; }
        public string PseudonymeJ { get; set; }
        public TimeSpan TempsDeCourseE { get; set; }
        public string PseudonymeE { get; set; }

        public bool DisconnectedT { get; set; }
        const int BUFFER_SIZE = 2048;
        //Server Serveur { get; set; }
        TcpClient Client { get; set; }
        byte[] ReadBuffer { get; set; }
        public bool enemiConnecté { get; private set; }
        /*public */
        MemoryStream readStream, writeStream;
        public Vector3 PositionEnnemi { get; private set; }
        public Matrix MatriceMondeEnnemi { get; private set; }
        /*public */
        BinaryReader reader;
        /*public */
        BinaryWriter writer;
        public bool EnnemiPrêtÀJouer { get; private set; }
        public bool EnnemiEstArrivé { get; private set; }

        public Réseautique(Game game,/*Server serveur,*/ string ip, int port)
            :base(game)
        {
            PseudonymeE = "ORDI";
            TempsDeCourseE = new TimeSpan(0, 0, 20);

            //Serveur = serveur;
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
            SendData(/*Serveur.*/GetDataFromMemoryStream(writeStream));
            Client.GetStream().BeginRead(ReadBuffer, 0, BUFFER_SIZE, StreamReceived, null);
        }

        public void SendMatriceMonde(Matrix m)
        {
            writeStream.Position = 0;
            writer.Write(((byte)Protocoles.PlayerMoved));
            writer.Write(m.M11);
            writer.Write(m.M12);
            writer.Write(m.M13);
            writer.Write(m.M14);
            writer.Write(m.M21);
            writer.Write(m.M22);
            writer.Write(m.M23);
            writer.Write(m.M24);
            writer.Write(m.M31);
            writer.Write(m.M32);
            writer.Write(m.M33);
            writer.Write(m.M34);
            writer.Write(m.M41);
            writer.Write(m.M42);
            writer.Write(m.M43);
            writer.Write(m.M44);

            SendData(/*Serveur.*/GetDataFromMemoryStream(writeStream));
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
                //MessageBox.Show(ex.Message + "salut cest ici l'erreur");
                DisconnectedT = true;
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
                    writeStream.Position = 0;
                    writer.Write((byte)Protocoles.Connected);
                    SendData(/*Serveur.*/GetDataFromMemoryStream(writeStream));
                }

            }
            else if (p == Protocoles.Disconnected)
            {
                enemiConnecté = false;
                DisconnectedT = true;
            }
            else if (p == Protocoles.PlayerMoved)
            {
                float m11 = reader.ReadSingle();
                float m12 = reader.ReadSingle();
                float m13 = reader.ReadSingle();
                float m14 = reader.ReadSingle();
                float m21 = reader.ReadSingle();
                float m22 = reader.ReadSingle();
                float m23 = reader.ReadSingle();
                float m24 = reader.ReadSingle();
                float m31 = reader.ReadSingle();
                float m32 = reader.ReadSingle();
                float m33 = reader.ReadSingle();
                float m34 = reader.ReadSingle();
                float m41 = reader.ReadSingle();
                float m42 = reader.ReadSingle();
                float m43 = reader.ReadSingle();
                float m44 = reader.ReadSingle();

                MatriceMondeEnnemi = new Matrix(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);
                //si c'est pas legit, envoie nouveau protocole.
            }
            else if (p == Protocoles.PositionInitiale)
            {
                float X = reader.ReadSingle();
                float Y = reader.ReadSingle();
                float Z = reader.ReadSingle();
                PositionEnnemi = new Vector3(X, Y, Z);
            }
            else if (p == Protocoles.ReadyToPlayChanged)
            {
                EnnemiPrêtÀJouer = reader.ReadBoolean();
            }
            else if (p == Protocoles.HasArrivedToEnd)
            {
                EnnemiEstArrivé = reader.ReadBoolean();
                TempsDeCourseE = new TimeSpan(reader.ReadInt64());
                PseudonymeE = reader.ReadString();
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
        #region méthodes publiques

        public void SendPosIni(Vector3 position)
        {
            writeStream.Position = 0;
            writer.Write((byte)Protocoles.PositionInitiale);
            writer.Write(position.X);
            writer.Write(position.Y);
            writer.Write(position.Z);
            SendData(/*Serveur.*/GetDataFromMemoryStream(writeStream));
        }
        public void SendTerminé(bool val, TimeSpan tempsDeCourse, string pseudonyme)
        {
            writeStream.Position = 0;
            writer.Write((byte)Protocoles.HasArrivedToEnd);
            writer.Write(val);
            writer.Write(tempsDeCourse.Ticks);
            writer.Write(pseudonyme);
            SendData(/*Serveur.*/GetDataFromMemoryStream(writeStream));
        }
        public void SendPrêtJeu(bool val)
        {
            writeStream.Position = 0;
            writer.Write((byte)Protocoles.ReadyToPlayChanged);
            writer.Write(val);
            SendData(/*Serveur.*/GetDataFromMemoryStream(writeStream));
        }
        public void SendDisconnect()
        {
            //Client.Close(); //test
            writeStream.Position = 0;
            writer.Write((byte)Protocoles.Disconnected);
            SendData(/*Serveur.*/GetDataFromMemoryStream(writeStream));
        }
        #endregion
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

            ms.Read(result, 0, bytesWritten);

            return result;
        }
        public void Close()
        {
            Client.Close();
        }
        public void Reset(string ip, int port)
        {
            PseudonymeE = "ORDI";
            TempsDeCourseE = new TimeSpan(0, 0, 10);

            //Serveur = serveur;
            EnnemiPrêtÀJouer = false;

            //Client = new TcpClient();
            //readStream = new MemoryStream();
           // writeStream = new MemoryStream();

            enemiConnecté = false;

          //  reader = new BinaryReader(readStream);
         //   writer = new BinaryWriter(writeStream);
         //   Client.NoDelay = true;
        //    Client.Connect(ip, port);

        //    ReadBuffer = new byte[BUFFER_SIZE];

       //     writeStream.Position = 0;
       //     writer.Write((byte)Protocoles.Connected);
            //SendData(/*Serveur.*/GetDataFromMemoryStream(writeStream));
            //Client.GetStream().BeginRead(ReadBuffer, 0, BUFFER_SIZE, StreamReceived, null);

            DisconnectedT = false;
            EnnemiPrêtÀJouer = false;
            EnnemiEstArrivé = false;
        }
    }
}
