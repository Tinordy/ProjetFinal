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
using System.Windows.Forms;
using System.IO;

namespace AtelierXNA
{
    enum �tatsJeu { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, JEU, CHOIX_PROFILE, ENTR�E_PORT_SERVEUR, ENTR�E_PORT_CLIENT, CONNECTION }
    public class Jeu : Microsoft.Xna.Framework.GameComponent
    {
        const int BUFFER_SIZE = 2048;
        �tatsJeu �tat { get; set; }
        MenuPrincipal MenuPrincipal { get; set; }
        MenuOption MenuDesOptions { get; set; }
        MenuLan MenuNetwork { get; set; }
        MenuProfile MenuChoixProfile { get; set; }
        MenuIPServeur MenuServeur { get; set; }
        MenuIPClient MenuClient { get; set; }
        Server Serveur { get; set; }
        TcpClient Client { get; set; }
        byte[] ReadBuffer { get; set; }
        bool enemiConnect� { get; set; }
        MemoryStream readStream, writeStream;

        BinaryReader reader;
        BinaryWriter writer;

        public Jeu(Game game)
            : base(game)
        {
            Cr�erMenuPrincipal();
            Cr�erMenuLan();
            Cr�erMenuOption();
            Cr�erMenuProfile();
            Cr�erMenusIP();
        }



        public override void Initialize()
        {
            �tat = �tatsJeu.MENU_PRINCIPAL;
            MenuPrincipal.Enabled = true;

        }

        public override void Update(GameTime gameTime)
        {
            G�rerTransition();
        }

        void G�rerTransition()
        {
            switch (�tat)
            {
                case �tatsJeu.MENU_PRINCIPAL:
                    G�rerTransitionMenuPrincipal();
                    break;
                case �tatsJeu.MENU_OPTION:
                    G�rerTransitionMenuOption();
                    break;
                case �tatsJeu.CHOIX_LAN:
                    G�rerTransitionMenuChoixLan();
                    break;
                case �tatsJeu.CHOIX_PROFILE:
                    G�rerTransitionMenuProfile();
                    break;
                case �tatsJeu.ENTR�E_PORT_SERVEUR:
                    G�rerTransitionMenuServeur();
                    break;
                case �tatsJeu.ENTR�E_PORT_CLIENT:
                    G�rerTransitionMenuClient();
                    break;
                case �tatsJeu.CONNECTION:
                    G�rerTransitionConnection();
                    break;
            }

        }



        private void G�rerTransitionConnection()
        {
            if(Serveur.connectedClients == 2)
            {
                �tat = �tatsJeu.CHOIX_PROFILE;
                MenuServeur.Enabled = false;
                MenuClient.Enabled = false;
                MenuChoixProfile.Enabled = true;
            }
        }

        private void G�rerTransitionMenuProfile()
        {
            switch(MenuChoixProfile.Choix)
            {
                case ChoixMenu.EN_ATTENTE:
                    break;
                case ChoixMenu.JOUER:
                    //retirer tous les menus des components?
                    MenuChoixProfile.Enabled = false;
                    D�marrerLeJeu();
                    break;
            }
        }



        void G�rerTransitionMenuPrincipal()
        {
            switch (MenuPrincipal.Choix)
            {
                case ChoixMenu.EN_ATTENTE:
                    break;
                case ChoixMenu.JOUER:
                    �tat = �tatsJeu.CHOIX_LAN;
                    MenuPrincipal.Enabled = false;
                    MenuNetwork.Enabled = true;
                    break;
                case ChoixMenu.OPTION:
                    �tat = �tatsJeu.MENU_OPTION;
                    MenuPrincipal.D�sactiverBoutons();
                    MenuDesOptions.Enabled = true;
                    break;
                case ChoixMenu.QUITTER:
                    Game.Exit(); // fonction?
                    break;
            }
        }
        void G�rerTransitionMenuOption()
        {
            switch (MenuDesOptions.Choix)
            {
                case ChoixMenu.EN_ATTENTE:
                    break;
                case ChoixMenu.RETOUR:
                    �tat = �tatsJeu.MENU_PRINCIPAL;
                    MenuDesOptions.Enabled = false;
                    MenuPrincipal.D�sactiverBoutons(); //changer nom
                    break;
            }
        }
        void G�rerTransitionMenuChoixLan()
        {
            switch (MenuNetwork.Choix)
            {
                case ChoixMenu.EN_ATTENTE:
                    break;
                case ChoixMenu.SOLO:
                    �tat = �tatsJeu.CHOIX_PROFILE;
                    MenuNetwork.Enabled = false;
                    MenuChoixProfile.Enabled = true;
                    break;
                case ChoixMenu.REJOINDRE:
                    �tat = �tatsJeu.ENTR�E_PORT_CLIENT; 
                    MenuNetwork.Enabled = false;
                    MenuClient.Enabled = true;
                    break;
                case ChoixMenu.SERVEUR:
                    �tat = �tatsJeu.ENTR�E_PORT_SERVEUR;
                    MenuNetwork.Enabled = false;
                    MenuServeur.Enabled = true; 
                    break;
                case ChoixMenu.RETOUR:
                    �tat = �tatsJeu.MENU_PRINCIPAL;
                    MenuNetwork.Enabled = false;
                    MenuPrincipal.Enabled = true;
                    break;
            }
        }
        void G�rerTransitionMenuServeur()
        {
            switch (MenuServeur.Choix)
            {
                case ChoixMenu.EN_ATTENTE:
                    break;
                case ChoixMenu.CONNECTION:
                    �tat = �tatsJeu.CONNECTION;
                    ConnectionAuServeur(MenuServeur.IP, MenuServeur.Port);
                    break;
            }
        }
        private void G�rerTransitionMenuClient()
        {
            switch(MenuClient.Choix)
            {
                case ChoixMenu.EN_ATTENTE:
                    break;
                case ChoixMenu.CONNECTION:
                    �tat = �tatsJeu.CHOIX_PROFILE;
                    ConnectionAuServeur(MenuClient.IP, MenuClient.Port);
                    break;
            }
        }
        private void ConnectionAuServeur(string ip, int port)
        {
            Serveur = new Server(port); 
            Client = new TcpClient();
            readStream = new MemoryStream();
            writeStream = new MemoryStream();

            enemiConnect� = false;

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
        private void D�marrerLeJeu()
        {
            //d�marre le jeu peu importe si c'est un simple jouer ou multijoueur... d�riv�e de la classe jeu?
        }
        #region Cr�ation Des Menus
        void Cr�erMenuPrincipal()
        {
            MenuPrincipal = new MenuPrincipal(Game);
            Game.Components.Add(MenuPrincipal);
        }
        void Cr�erMenuOption()
        {
            MenuDesOptions = new MenuOption(Game);
            Game.Components.Add(MenuDesOptions);
        }
        void Cr�erMenuLan()
        {
            MenuNetwork = new MenuLan(Game);
            Game.Components.Add(MenuNetwork);
        }
        void Cr�erMenuProfile()
        {
            MenuChoixProfile = new MenuProfile(Game);
            Game.Components.Add(MenuChoixProfile);
        }
        private void Cr�erMenusIP()
        {
            MenuServeur = new MenuIPServeur(Game);
            Game.Components.Add(MenuServeur);
            MenuClient = new MenuIPClient(Game);
            Game.Components.Add(MenuClient);
            //une fonction?
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
                if (!enemiConnect�)
                {
                    enemiConnect� = true;
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
                enemiConnect� = false;
            }
            else if (p == Protocoles.PlayerMoved)
            {

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
        #endregion

    }
}
