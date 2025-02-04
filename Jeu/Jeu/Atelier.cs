﻿using System;
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
using System.Threading;

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
        Server Server { get; set; }
        int ancienNbDeClients { get; set; }

        // server related properties

        string IP = "172.22.157.39";
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
            //PériphériqueGraphique.PreferredBackBufferHeight = Window.ClientBounds.Height;
            //PériphériqueGraphique.PreferredBackBufferWidth = Window.ClientBounds.Width;
            //PériphériqueGraphique.IsFullScreen = true;
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
            //enemy = new Maison(this, 1f, Vector3.Zero, Vector3.Zero, INTERVALLE_MAJ_STANDARD);
            //player = new Maison(this, 1f, Vector3.Zero, Vector3.Zero, new Vector3(5f, 5f, 5f), "PlayerPaper", "EnemyPaper", INTERVALLE_MAJ_STANDARD);
            //Vector3 positionCaméra = new Vector3(100, 100, -100);
            Vector3 positionCaméra = new Vector3(10, 0, 20);
            Vector3 cibleCaméra = new Vector3(10, 0, 10);
            //ListeSections = new List<Section>();

            Components.Add(GestionInput);

            //Sections = new List<Section>();
            Components.Add(new Afficheur3D(this));

            //Components.Add(new Volant(this, 0.01f));

            Components.Add(new Jeu(this));
            //Components.Add(new BannièreArrivée(this, 1f, Vector3.Zero, Vector3.Zero, 0.01f));
            //Components.Add(new BannièreDarrivée(this, 1f, Vector3.Zero, new Vector3(10,0,10), Color.Red));
            //Components.Add(new JeuTest(this));

            //Components.Add(new ArrièrePlanDéroulant(this, "CielÉtoilé", INTERVALLE_MAJ_STANDARD));
            //for (int i = 0; i < 2; ++i)
            //{
            //    for (int j = 0; j < 2; ++j)
            //    {
            //        Section newSection = new Section(this, new Vector2(200 * i, 100 * j), new Vector2(200, 200), 1f, Vector3.Zero, Vector3.Zero, new Vector3(200, 25, 200), new string[] { "Herbe", "Sable" }, INTERVALLE_MAJ_STANDARD);
            //        //Sections.Add(newSection);
            //        ListeSections.Add(newSection);
            //    }
            //}
            //foreach (Section s in ListeSections)
            //{
            //    Components.Add(s);
            //}

            //Components.Add(new Terrain(this, 1f, Vector3.Zero, Vector3.Zero, new Vector3(256, 25, 256), "GrandeCarte", "DétailsTerrain", 5, INTERVALLE_MAJ_STANDARD));
            //Components.Add(new Terrain(this, 1f, Vector3.Zero, Vector3.Zero, new Vector3(200, 25, 200), "CarteTest", "DétailsTerrain", 5, INTERVALLE_MAJ_STANDARD));
            //Components.Add(new AfficheurFPS(this, "Arial20", Color.Red, INTERVALLE_CALCUL_FPS));


            //Services.AddService(typeof(Random), new Random());
            Services.AddService(typeof(RessourcesManager<SpriteFont>), new RessourcesManager<SpriteFont>(this, "Fonts"));
            Services.AddService(typeof(RessourcesManager<Texture2D>), new RessourcesManager<Texture2D>(this, "Textures"));
            Services.AddService(typeof(RessourcesManager<Model>), new RessourcesManager<Model>(this, "Modèles"));
            //Caméra et piste
            CaméraJeu = new CaméraSubjective(this, positionCaméra, cibleCaméra, Vector3.Up, INTERVALLE_MAJ_STANDARD);
            //Components.Add(CaméraJeu);
            //Services.AddService(typeof(Caméra), CaméraJeu);
            //Components.Add(new PisteSectionnée(this, 1f, Vector3.Zero, Vector3.Zero, INTERVALLE_MAJ_STANDARD, 20000, 20000));
            Services.AddService(typeof(DataPiste), new DataPiste("SplineX.txt", "SplineY.txt"));

            GestionSprites = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), GestionSprites);
            //Components.Add(player);
            //Server = new Server(PORT);           
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            //client = new TcpClient();
            //client.NoDelay = true;
            //client.Connect(IP, PORT);

            //readBuffer = new byte[BUFFER_SIZE];

            //writeStream.Position = 0;
            //writer.Write((byte)Protocoles.Connected);
            //writer.Write(player.Position.X);
            //writer.Write(player.Position.Y);
            //writer.Write(player.Position.Z);
            //SendData(Server.GetDataFromMemoryStream(writeStream));

            //client.GetStream().BeginRead(readBuffer, 0, BUFFER_SIZE, StreamReceived, null);


        }

        void UpdateLan(GameTime gameTime)
        {
            float valeurTranslation = 1f;
            Vector3 iPosition = new Vector3(player.Position.X, player.Position.Y, player.Position.Z);
            Vector3 nPosition = iPosition;
            if (GestionInput.EstEnfoncée(Microsoft.Xna.Framework.Input.Keys.I))
            {
                nPosition = new Vector3(player.Position.X, player.Position.Y, player.Position.Z + valeurTranslation);
            }
            if (GestionInput.EstEnfoncée(Microsoft.Xna.Framework.Input.Keys.K))
            {
                nPosition = new Vector3(player.Position.X, player.Position.Y, player.Position.Z - valeurTranslation);
            }
            if (GestionInput.EstEnfoncée(Microsoft.Xna.Framework.Input.Keys.J))
            {
                nPosition = new Vector3(player.Position.X + valeurTranslation, player.Position.Y, player.Position.Z);
            }
            if (GestionInput.EstEnfoncée(Microsoft.Xna.Framework.Input.Keys.L))
            {
                nPosition = new Vector3(player.Position.X - valeurTranslation, player.Position.Y, player.Position.Z);
            }


            Vector3 delta = Vector3.Subtract(nPosition, iPosition);

            if (delta != Vector3.Zero)
            {
                //player.Position = Server.VérificationPositionServeur(nPosition);
                //player.CalculerMatriceMonde();
                //writeStream.Position = 0;
                //writer.Write((byte)Protocoles.PlayerMoved);
                //writer.Write(player.Position.X);
                //writer.Write(player.Position.Y);
                //writer.Write(player.Position.Z);
                //SendData(Server.GetDataFromMemoryStream(writeStream));

            }
        }

        protected override void Update(GameTime gameTime)
        {
            //DéconnectionDeJoueur();
            GérerClavier();
            base.Update(gameTime);
            TempsÉcouléDepuisMAJ += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (TempsÉcouléDepuisMAJ >= INTERVALLE_MAJ_STANDARD)
            {
                //UpdateLan(gameTime);

                TempsÉcouléDepuisMAJ = 0;

            }

        }

        private void DéconnectionDeJoueur()
        {

            if (Server.connectedClients < ancienNbDeClients)
            {
                Components.Remove(enemy);
            }
            ancienNbDeClients = Server.connectedClients;
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
                    enemyConnected = true;
                    //float X = reader.ReadSingle();
                    //float Y = reader.ReadSingle();
                    //float Z = reader.ReadSingle();
                    //enemy.Position = new Vector3(X, Y, Z);
                    //enemy = new Maison(this, 1f, Vector3.Zero, new Vector3(X,Y,Z), new Vector3(5f, 5f, 5f), "PlayerPaper", "EnemyPaper", INTERVALLE_MAJ_STANDARD);
                    //Components.Add(enemy);


                    //writeStream.Position = 0;
                    //writer.Write((byte)Protocoles.Connected);
                    //writer.Write(player.Position.X);
                    //writer.Write(player.Position.Y);
                    //writer.Write(player.Position.Z);
                    //SendData(Server.GetDataFromMemoryStream(writeStream));
                }

            }
            else if (p == Protocoles.Disconnected)
            {
                Components.Remove(enemy);
                enemyConnected = false;
                enemy.Enabled = false;
            }
            else if (p == Protocoles.PlayerMoved)
            {
                float X = reader.ReadSingle();
                float Y = reader.ReadSingle();
                float Z = reader.ReadSingle();
                enemy.Position = new Vector3(X, Y, Z);
                enemy.CalculerMatriceMonde();
                player.CalculerMatriceMonde();

            }
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

