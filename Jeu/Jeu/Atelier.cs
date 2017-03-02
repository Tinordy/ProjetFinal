using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Windows.Forms;

namespace AtelierXNA
{
    public class Atelier : Microsoft.Xna.Framework.Game
    {
        const float INTERVALLE_CALCUL_FPS = 1f;
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;
        GraphicsDeviceManager PériphériqueGraphique { get; set; }
        SpriteBatch GestionSprites { get; set; }
        List<Section> Sections { get; set; }
        Caméra CaméraJeu { get; set; }
        InputManager GestionInput { get; set; }
        DataPiste DonnéesPiste { get; set; }
        List<Section> ListeSections { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }
        TcpClient client;

        // server related properties

        string IP = "127.0.0.1";
        int PORT = 5001;
        int BUFFER_SIZE = 2048;
        byte[] readBuffer;
        MemoryStream readStream, writeStream;

        BinaryReader reader;
        BinaryWriter writer;

        Maison player;
        Maison enemy;

        bool enemyConnected = false;

        //
        public Atelier()
        {
            PériphériqueGraphique = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            PériphériqueGraphique.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            IsMouseVisible = true;

        }

        protected override void Initialize()
        {
            GestionInput = new InputManager(this);
            Services.AddService(typeof(InputManager), GestionInput);
            //serveur 



            readStream = new MemoryStream();
            writeStream = new MemoryStream();

            reader = new BinaryReader(readStream);
            writer = new BinaryWriter(writeStream);

            //
            //joueur 
            enemy = new Maison(this, 1f, Vector3.Zero, Vector3.Zero, INTERVALLE_MAJ_STANDARD);
            player = new Maison(this, 1f, Vector3.Zero, Vector3.Zero, new Vector3(5f, 5f, 5f), "PlayerPaper", "EnemyPaper", INTERVALLE_MAJ_STANDARD);
            Vector3 positionCaméra = new Vector3(200, 10, 200);
            Vector3 cibleCaméra = new Vector3(10, 0, 10);
            ListeSections = new List<Section>();

            Components.Add(GestionInput);
            CaméraJeu = new CaméraSubjective(this, positionCaméra, cibleCaméra, Vector3.Up, INTERVALLE_MAJ_STANDARD);
            Components.Add(CaméraJeu);
            Sections = new List<Section>();
            Components.Add(new Afficheur3D(this));
            Components.Add(new ArrièrePlanDéroulant(this, "CielÉtoilé", INTERVALLE_MAJ_STANDARD));
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    Section newSection = new Section(this, new Vector2(200 * i, 100 * j), new Vector2(200, 200), 1f, Vector3.Zero, Vector3.Zero, new Vector3(200, 25, 200), new string[] { "Herbe", "Sable" }, INTERVALLE_MAJ_STANDARD);
                    Sections.Add(newSection);
                    ListeSections.Add(newSection);
                }
            }
            foreach (Section s in ListeSections)
            {
                Components.Add(s);
            }

            //Components.Add(new Terrain(this, 1f, Vector3.Zero, Vector3.Zero, new Vector3(256, 25, 256), "GrandeCarte", "DétailsTerrain", 5, INTERVALLE_MAJ_STANDARD));
            //Components.Add(new Terrain(this, 1f, Vector3.Zero, Vector3.Zero, new Vector3(200, 25, 200), "CarteTest", "DétailsTerrain", 5, INTERVALLE_MAJ_STANDARD));
            Components.Add(new AfficheurFPS(this, "Arial20", Color.Red, INTERVALLE_CALCUL_FPS));
            Components.Add(new Piste(this, 1f, Vector3.Zero, Vector3.Zero, INTERVALLE_MAJ_STANDARD, 20000, 20000));

            //Services.AddService(typeof(Random), new Random());
            Services.AddService(typeof(RessourcesManager<SpriteFont>), new RessourcesManager<SpriteFont>(this, "Fonts"));
            Services.AddService(typeof(RessourcesManager<Texture2D>), new RessourcesManager<Texture2D>(this, "Textures"));
            Services.AddService(typeof(Caméra), CaméraJeu);
            Services.AddService(typeof(DataPiste), new DataPiste("SplineX.txt", "SplineY.txt"));
            GestionSprites = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), GestionSprites);
            Components.Add(player);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            client = new TcpClient();
            client.NoDelay = true;
            client.Connect(IP, PORT);

            readBuffer = new byte[BUFFER_SIZE];
            client.GetStream().BeginRead(readBuffer, 0, BUFFER_SIZE, StreamReceived, null);

        }

        void UpdateLan(GameTime gameTime)
        {
            Vector3 iPosition = new Vector3(player.Position.X, player.Position.Y, player.Position.Z);
            Vector3 nPosition = iPosition;
            if(GestionInput.EstEnfoncée(Microsoft.Xna.Framework.Input.Keys.W))
            {
                nPosition = new Vector3(player.Position.X, player.Position.Y, player.Position.Z + 0.01f);
            }
            if (GestionInput.EstEnfoncée(Microsoft.Xna.Framework.Input.Keys.S))
            {
                nPosition = new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 0.01f);
            }
            if (GestionInput.EstEnfoncée(Microsoft.Xna.Framework.Input.Keys.A))
            {
                nPosition = new Vector3(player.Position.X + 0.01f, player.Position.Y, player.Position.Z);
            }
            if (GestionInput.EstEnfoncée(Microsoft.Xna.Framework.Input.Keys.D))
            {
                nPosition = new Vector3(player.Position.X - 0.01f, player.Position.Y, player.Position.Z);
            }


            Vector3 delta = Vector3.Subtract(nPosition, iPosition);

            if (delta != Vector3.Zero)
            {
                player.Position = nPosition;
                player.CalculerMatriceMonde();
                writeStream.Position = 0;
                writer.Write((byte)Protocoles.PlayerMoved);
                writer.Write(delta.X);
                writer.Write(delta.Y);
                writer.Write(delta.Z);
                SendData(GetDataFromMemoryStream(writeStream));


            }
        }

        protected override void Update(GameTime gameTime)
        {
            GérerClavier();
            UpdateLan(gameTime);
            base.Update(gameTime);
        }

        void StreamReceived(IAsyncResult ar)
        {
            int bytesRead = 0;

            try
            {
                lock (client.GetStream())
                {
                    bytesRead = client.GetStream().EndRead(ar);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "1");
            }

            if (bytesRead == 0)
            {
                client.Close();
                return;
            }

            byte[] data = new byte[bytesRead];

            for (int cpt = 0; cpt < bytesRead; cpt++)
                data[cpt] = readBuffer[cpt];

            ProcessData(data);


            client.GetStream().BeginRead(readBuffer, 0, BUFFER_SIZE, StreamReceived, null);
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
                if (!enemyConnected)
                {
                    byte id = reader.ReadByte();
                    string ip = reader.ReadString();
                    enemyConnected = true;
                    enemy = new Maison(this, 1f, Vector3.Zero, new Vector3(0, 0, 5), new Vector3(5f, 5f, 5f), "PlayerPaper", "EnemyPaper", INTERVALLE_MAJ_STANDARD);
                    Components.Add(enemy);
                    

                    writeStream.Position = 0;
                    writer.Write((byte)Protocoles.Connected);
                    SendData(GetDataFromMemoryStream(writeStream));
                }

            }
            else if (p == Protocoles.Disconnected)
            {
                byte id = reader.ReadByte();
                string ip = reader.ReadString();
                enemyConnected = false;
            }
            else if (p == Protocoles.PlayerMoved)
            {
                float X = reader.ReadSingle();
                float Y = reader.ReadSingle();
                float Z = reader.ReadSingle();
                byte id = reader.ReadByte();
                string ip = reader.ReadString();
                enemy.Position = new Vector3(enemy.Position.X + X, enemy.Position.Y + Y, enemy.Position.Z + Z);
                enemy.CalculerMatriceMonde();
                
            }
            //}
            //catch (Exception exception)
            //{
            //    MessageBox.Show(exception.Message + "2");
            //}
        }

        private byte[] GetDataFromMemoryStream(MemoryStream ms)
        {
            byte[] result;
            lock (ms)
            {
                int bytesWritten = (int)ms.Position;
                result = new byte[bytesWritten];

                ms.Position = 0;
                ms.Read(result, 0, bytesWritten);
            }

            return result;
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

            }
        }
        private void GérerClavier()
        {
            if (GestionInput.EstEnfoncée(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                Exit();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);
        }
    }
}

