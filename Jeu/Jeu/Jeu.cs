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
   enum ÉtatsJeu { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, JEU, CHOIX_PROFILE, ENTRÉE_PORT_SERVEUR, ENTRÉE_PORT_CLIENT, CONNECTION, ATTENTE_JOUEURS }
   enum ÉtatsJoueur { SOLO, SERVEUR, CLIENT }
   public class Jeu : Microsoft.Xna.Framework.GameComponent
   {
      const int BUFFER_SIZE = 2048;
      ÉtatsJeu État { get; set; }
      ÉtatsJoueur ÉtatJoueur { get; set; }
      MenuPrincipal MenuPrincipal { get; set; }
      MenuOption MenuDesOptions { get; set; }
      MenuLan MenuNetwork { get; set; }
      MenuProfile MenuChoixProfile { get; set; }
      MenuIPServeur MenuServeur { get; set; }
      MenuIPClient MenuClient { get; set; }

      Server Serveur { get; set; }
      TcpClient Client { get; set; }
      byte[] ReadBuffer { get; set; }
      bool enemiConnecté { get; set; }
      MemoryStream readStream, writeStream;

      BinaryReader reader;
      BinaryWriter writer;

      Voiture Joueur { get; set; }
      Voiture Ennemi { get; set; }
      bool EnnemiPrêtÀJouer { get; set; }
      public Jeu(Game game)
          : base(game)
      {
         CréerMenuPrincipal();
         CréerMenuLan();
         CréerMenuOption();
         CréerMenuProfile();
         CréerMenusIP();
      }



      public override void Initialize()
      {
         EnnemiPrêtÀJouer = false;
         État = ÉtatsJeu.MENU_PRINCIPAL;
         MenuPrincipal.Enabled = true;
      }

      public override void Update(GameTime gameTime)
      {
         GérerTransition();
         GérerÉtat();
      }

      private void GérerÉtat()
      {
         switch (État)
         {
            case ÉtatsJeu.JEU:
               //Collision, gagnant..???
               break;
         }


      }

      void GérerTransition()
      {
         switch (État)
         {
            case ÉtatsJeu.MENU_PRINCIPAL:
               GérerTransitionMenuPrincipal();
               break;
            case ÉtatsJeu.MENU_OPTION:
               GérerTransitionMenuOption();
               break;
            case ÉtatsJeu.CHOIX_LAN:
               GérerTransitionMenuChoixLan();
               break;
            case ÉtatsJeu.CHOIX_PROFILE:
               GérerTransitionMenuProfile();
               break;
            case ÉtatsJeu.ENTRÉE_PORT_SERVEUR:
               GérerTransitionMenuServeur();
               break;
            case ÉtatsJeu.ENTRÉE_PORT_CLIENT:
               GérerTransitionMenuClient();
               break;
            case ÉtatsJeu.CONNECTION:
               GérerTransitionConnection();
               break;
            case ÉtatsJeu.ATTENTE_JOUEURS:
               GérerTransitionAttenteJoueurs();
               break;
         }

      }

      private void GérerTransitionAttenteJoueurs()
      {
         switch (ÉtatJoueur)
         {
            case ÉtatsJoueur.SOLO:
               MenuChoixProfile.ActiverBtnDémarrer();
               État = ÉtatsJeu.CHOIX_PROFILE;
               break;
            case ÉtatsJoueur.CLIENT:
               if (EnnemiPrêtÀJouer)
               {
                  //INITIALISATION?
                  DémarrerLeJeu();
                  État = ÉtatsJeu.JEU;
               }
               break;
            case ÉtatsJoueur.SERVEUR:
               if (EnnemiPrêtÀJouer)
               {
                  État = ÉtatsJeu.CHOIX_PROFILE;
                  MenuChoixProfile.ActiverBtnDémarrer();
               }
               break;
         }
      }

      private void GérerTransitionConnection()
      {
         if (Serveur.connectedClients == 2)
         {
            État = ÉtatsJeu.CHOIX_PROFILE;
            MenuServeur.Enabled = false;
            MenuChoixProfile.Enabled = true;
         }
      }

      private void GérerTransitionMenuProfile()
      {
         switch (MenuChoixProfile.Choix)
         {
            case ChoixMenu.EN_ATTENTE:
               break;
            case ChoixMenu.VALIDATION:
               if (ÉtatJoueur == ÉtatsJoueur.CLIENT)
               {
                  writeStream.Position = 0;
                  writer.Write((byte)Protocoles.ReadyToPlayChanged);
                  writer.Write(true);
               }
               État = ÉtatsJeu.ATTENTE_JOUEURS;
               break;
            case ChoixMenu.JOUER:
               //retirer tous les menus des components?
               //INITIALISATION??
               État = ÉtatsJeu.JEU;
               MenuChoixProfile.Enabled = false;
               DémarrerLeJeu();
               break;
         }
      }



      void GérerTransitionMenuPrincipal()
      {
         switch (MenuPrincipal.Choix)
         {
            case ChoixMenu.EN_ATTENTE:
               break;
            case ChoixMenu.JOUER:
               État = ÉtatsJeu.CHOIX_LAN;
               MenuPrincipal.Enabled = false;
               MenuNetwork.Enabled = true;
               break;
            case ChoixMenu.OPTION:
               État = ÉtatsJeu.MENU_OPTION;
               MenuPrincipal.DésactiverBoutons();
               MenuDesOptions.Enabled = true;
               break;
            case ChoixMenu.QUITTER:
               Game.Exit(); // fonction?
               break;
         }
      }
      void GérerTransitionMenuOption()
      {
         switch (MenuDesOptions.Choix)
         {
            case ChoixMenu.EN_ATTENTE:
               break;
            case ChoixMenu.RETOUR:
               État = ÉtatsJeu.MENU_PRINCIPAL;
               MenuDesOptions.Enabled = false;
               MenuPrincipal.DésactiverBoutons(); //changer nom
               break;
         }
      }
      void GérerTransitionMenuChoixLan()
      {
         switch (MenuNetwork.Choix)
         {
            case ChoixMenu.EN_ATTENTE:
               break;
            case ChoixMenu.SOLO:
               État = ÉtatsJeu.CHOIX_PROFILE;
               MenuNetwork.Enabled = false;
               MenuChoixProfile.Enabled = true;
               ÉtatJoueur = ÉtatsJoueur.SOLO;
               ConnectionAuServeur("127.0.0.1", 5001); //ok?
               break;
            case ChoixMenu.REJOINDRE:
               État = ÉtatsJeu.ENTRÉE_PORT_CLIENT;
               MenuNetwork.Enabled = false;
               MenuClient.Enabled = true;
               ÉtatJoueur = ÉtatsJoueur.CLIENT;
               break;
            case ChoixMenu.SERVEUR:
               État = ÉtatsJeu.ENTRÉE_PORT_SERVEUR;
               MenuNetwork.Enabled = false;
               MenuServeur.Enabled = true;
               ÉtatJoueur = ÉtatsJoueur.SERVEUR;
               break;
            case ChoixMenu.RETOUR:
               État = ÉtatsJeu.MENU_PRINCIPAL;
               MenuNetwork.Enabled = false;
               MenuPrincipal.Enabled = true;
               break;
         }
      }
      void GérerTransitionMenuServeur()
      {
         switch (MenuServeur.Choix)
         {
            case ChoixMenu.EN_ATTENTE:
               break;
            case ChoixMenu.CONNECTION:
               État = ÉtatsJeu.CONNECTION;
               ConnectionAuServeur(MenuServeur.IP, MenuServeur.Port);
               break;
         }
      }
      private void GérerTransitionMenuClient()
      {
         switch (MenuClient.Choix)
         {

            case ChoixMenu.EN_ATTENTE:
               break;
            case ChoixMenu.CONNECTION:
               État = ÉtatsJeu.CHOIX_PROFILE;
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
      private void DémarrerLeJeu()
      {
         //changer de caméra?
         Vector3 positionCaméra = new Vector3(200, 10, 200);
         Vector3 cibleCaméra = new Vector3(10, 0, 10);
         CaméraSubjective CaméraJeu = new CaméraSubjective(Game, positionCaméra, cibleCaméra, Vector3.Up, 0.01f);
         Game.Components.Add(CaméraJeu);
         Game.Services.AddService(typeof(Caméra), CaméraJeu);


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
      #region Création Des Menus
      void CréerMenuPrincipal()
      {
         MenuPrincipal = new MenuPrincipal(Game);
         Game.Components.Add(MenuPrincipal);
      }
      void CréerMenuOption()
      {
         MenuDesOptions = new MenuOption(Game);
         Game.Components.Add(MenuDesOptions);
      }
      void CréerMenuLan()
      {
         MenuNetwork = new MenuLan(Game);
         Game.Components.Add(MenuNetwork);
      }
      void CréerMenuProfile()
      {
         MenuChoixProfile = new MenuProfile(Game);
         Game.Components.Add(MenuChoixProfile);
      }
      private void CréerMenusIP()
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
      #endregion

   }
}
