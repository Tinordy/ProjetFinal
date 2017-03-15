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
   enum �tatsJeu { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, JEU, CHOIX_PROFILE, ENTR�E_PORT_SERVEUR, ENTR�E_PORT_CLIENT, CONNECTION, ATTENTE_JOUEURS }
   enum �tatsJoueur { SOLO, SERVEUR, CLIENT }
   public class Jeu : Microsoft.Xna.Framework.GameComponent
   {
      const int BUFFER_SIZE = 2048;
      �tatsJeu �tat { get; set; }
      �tatsJoueur �tatJoueur { get; set; }
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

      Voiture Joueur { get; set; }
      Voiture Ennemi { get; set; }
      bool EnnemiPr�t�Jouer { get; set; }
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
         EnnemiPr�t�Jouer = false;
         �tat = �tatsJeu.MENU_PRINCIPAL;
         MenuPrincipal.Enabled = true;
      }

      public override void Update(GameTime gameTime)
      {
         G�rerTransition();
         G�rer�tat();
      }

      private void G�rer�tat()
      {
         switch (�tat)
         {
            case �tatsJeu.JEU:
               //Collision, gagnant..???
               break;
         }


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
            case �tatsJeu.ATTENTE_JOUEURS:
               G�rerTransitionAttenteJoueurs();
               break;
         }

      }

      private void G�rerTransitionAttenteJoueurs()
      {
         switch (�tatJoueur)
         {
            case �tatsJoueur.SOLO:
               MenuChoixProfile.ActiverBtnD�marrer();
               �tat = �tatsJeu.CHOIX_PROFILE;
               break;
            case �tatsJoueur.CLIENT:
               if (EnnemiPr�t�Jouer)
               {
                  //INITIALISATION?
                  D�marrerLeJeu();
                  �tat = �tatsJeu.JEU;
               }
               break;
            case �tatsJoueur.SERVEUR:
               if (EnnemiPr�t�Jouer)
               {
                  �tat = �tatsJeu.CHOIX_PROFILE;
                  MenuChoixProfile.ActiverBtnD�marrer();
               }
               break;
         }
      }

      private void G�rerTransitionConnection()
      {
         if (Serveur.connectedClients == 2)
         {
            �tat = �tatsJeu.CHOIX_PROFILE;
            MenuServeur.Enabled = false;
            MenuChoixProfile.Enabled = true;
         }
      }

      private void G�rerTransitionMenuProfile()
      {
         switch (MenuChoixProfile.Choix)
         {
            case ChoixMenu.EN_ATTENTE:
               break;
            case ChoixMenu.VALIDATION:
               if (�tatJoueur == �tatsJoueur.CLIENT)
               {
                  writeStream.Position = 0;
                  writer.Write((byte)Protocoles.ReadyToPlayChanged);
                  writer.Write(true);
               }
               �tat = �tatsJeu.ATTENTE_JOUEURS;
               break;
            case ChoixMenu.JOUER:
               //retirer tous les menus des components?
               //INITIALISATION??
               �tat = �tatsJeu.JEU;
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
               �tatJoueur = �tatsJoueur.SOLO;
               ConnectionAuServeur("127.0.0.1", 5001); //ok?
               break;
            case ChoixMenu.REJOINDRE:
               �tat = �tatsJeu.ENTR�E_PORT_CLIENT;
               MenuNetwork.Enabled = false;
               MenuClient.Enabled = true;
               �tatJoueur = �tatsJoueur.CLIENT;
               break;
            case ChoixMenu.SERVEUR:
               �tat = �tatsJeu.ENTR�E_PORT_SERVEUR;
               MenuNetwork.Enabled = false;
               MenuServeur.Enabled = true;
               �tatJoueur = �tatsJoueur.SERVEUR;
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
         switch (MenuClient.Choix)
         {

            case ChoixMenu.EN_ATTENTE:
               break;
            case ChoixMenu.CONNECTION:
               �tat = �tatsJeu.CHOIX_PROFILE;
               MenuClient.Enabled = false;
               MenuChoixProfile.Enabled = true;
               ConnectionAuServeur(MenuClient.IP, MenuClient.Port);
               break;
         }
      }
      private void ConnectionAuServeur(string ip, int port)
      {
         Serveur = new Server(port);
         Game.Services.AddService(typeof(Server), Serveur);


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
         //changer de cam�ra?
         Vector3 positionCam�ra = new Vector3(200, 10, 200);
         Vector3 cibleCam�ra = new Vector3(10, 0, 10);
         Cam�raSubjective Cam�raJeu = new Cam�raSubjective(Game, positionCam�ra, cibleCam�ra, Vector3.Up, 0.01f);
         Game.Components.Add(Cam�raJeu);
         Game.Services.AddService(typeof(Cam�ra), Cam�raJeu);


         Joueur = new Voiture(Game, "unicorn", 20f, Vector3.Zero, Vector3.Zero, 0.01f);
         Game.Components.Add(Joueur);

         //INITIALISATION??


         //writeStream.Position = 0;
         //writer.Write((byte)Protocoles.PositionInitiale);
         //writer.Write(Joueur.Position.X);
         //writer.Write(Joueur.Position.Y);
         //writer.Write(Joueur.Position.Z);
         //SendData(Serveur.GetDataFromMemoryStream(writeStream));

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
            float X = reader.ReadSingle();
            float Y = reader.ReadSingle();
            float Z = reader.ReadSingle();

            //faire une fonction!
            Ennemi.Position = new Vector3(X, Y, Z);
         }
         else if (p == Protocoles.PositionInitiale)
         {
            float X = reader.ReadSingle();
            float Y = reader.ReadSingle();
            float Z = reader.ReadSingle();

            //Ennemi = new Maison(Game, 1f, Vector3.Zero, new Vector3(X, Y, Z), new Vector3(2, 2, 2), "brique1", "roof", 0.01f);
            //voiture mtn...

            writeStream.Position = 0;
            writer.Write((byte)Protocoles.PlayerMoved);
            writer.Write(Joueur.Position.X);
            writer.Write(Joueur.Position.Y);
            writer.Write(Joueur.Position.Z);
            SendData(Serveur.GetDataFromMemoryStream(writeStream));
         }
         else if (p == Protocoles.ReadyToPlayChanged)
         {
            EnnemiPr�t�Jouer = reader.ReadBoolean();
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
