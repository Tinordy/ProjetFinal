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
        //const int BUFFER_SIZE = 2048;
        ÉtatsJeu État { get; set; }
        ÉtatsJoueur ÉtatJoueur { get; set; }
        MenuPrincipal MenuPrincipal { get; set; }
        MenuOption MenuDesOptions { get; set; }
        MenuLan MenuNetwork { get; set; }
        MenuProfile MenuChoixProfile { get; set; }
        MenuIPServeur MenuServeur { get; set; }
        MenuIPClient MenuClient { get; set; }

        Server Serveur { get; set; }
        Réseautique NetworkManager { get; set; }
        Voiture Joueur { get; set; }
        Voiture Ennemi { get; set; }
        List<Section> Sections { get; set; }

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
                    //if player moved?
                    if(ÉtatJoueur != ÉtatsJoueur.SOLO)
                    {
                        Ennemi.AjusterPosition(NetworkManager.MatriceMondeEnnemi);
                    }

                    //Collision, gagnant..???
                    break;
            }


        }
        #region Transitions
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
                    if (NetworkManager.EnnemiPrêtÀJouer)
                    {
                        //INITIALISATION?
                        MenuChoixProfile.Enabled = false;
                        DémarrerLeJeu();
                        État = ÉtatsJeu.JEU;
                    }
                    break;
                case ÉtatsJoueur.SERVEUR:
                    if (NetworkManager.EnnemiPrêtÀJouer)
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
                        NetworkManager.writeStream.Position = 0;
                        NetworkManager.writer.Write((byte)Protocoles.ReadyToPlayChanged);
                        NetworkManager.writer.Write(true);
                        NetworkManager.SendData(Serveur.GetDataFromMemoryStream(NetworkManager.writeStream));
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
                case ChoixMenu.QUITTER:
                    État = ÉtatsJeu.CHOIX_LAN;
                    MenuServeur.Enabled = false;
                    MenuNetwork.Enabled = true;
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
                case ChoixMenu.QUITTER:
                    État = ÉtatsJeu.CHOIX_LAN;
                    MenuClient.Enabled = false;
                    MenuNetwork.Enabled = true;
                    break;
            }
        }
        #endregion
        private void ConnectionAuServeur(string ip, int port)
        {
            Serveur = new Server(port, ip);
            Game.Services.AddService(typeof(Server), Serveur); //nécessaire?
            NetworkManager = new Réseautique(Serveur, ip, port);
            Game.Services.AddService(typeof(Réseautique), NetworkManager);

        }
        #region initialisation du jeu
        private void DémarrerLeJeu()
        {
            //changer de caméra?
            CréerCaméra();
            CréerJoueur();
            if(ÉtatJoueur != ÉtatsJoueur.SOLO)
            {
                CréerEnnemi();
            }
            CréerEnvironnement();
        }

        private void CréerEnvironnement()
        {
            Sections = new List<Section>();
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    Section newSection = new Section(Game, new Vector2(200 * i, 100 * j), new Vector2(200, 200), 1f, Vector3.Zero, Vector3.Zero, new Vector3(200, 25, 200), new string[] { "Herbe", "Sable" }, 0.01f);
                    Sections.Add(newSection);
                    Game.Components.Add(newSection);
                }
            }
        }

        private void CréerEnnemi()
        {
            Ennemi = new Voiture(Game, "unicorn", 20f, Vector3.Zero, NetworkManager.PositionEnnemi, 1.01f); //Get choix de voiture??
            Game.Components.Add(Ennemi);
        }

        private void CréerJoueur()
        {

            Joueur = new Voiture(Game, "unicorn", 20f, Vector3.Zero, new Vector3(100,0,50), 0.01f);
            Game.Components.Add(Joueur);

            NetworkManager.writeStream.Position = 0;
            NetworkManager.writer.Write((byte)Protocoles.PositionInitiale);
            NetworkManager.writer.Write(Joueur.Position.X);
            NetworkManager.writer.Write(Joueur.Position.Y);
            NetworkManager.writer.Write(Joueur.Position.Z);
            NetworkManager.SendData(Serveur.GetDataFromMemoryStream(NetworkManager.writeStream));
        }

        private void CréerCaméra()
        {
            Vector3 positionCaméra = new Vector3(200, 10, 200);
            Vector3 cibleCaméra = new Vector3(10, 0, 10);
            CaméraSubjective CaméraJeu = new CaméraSubjective(Game, positionCaméra, cibleCaméra, Vector3.Up, 0.01f);
            Game.Components.Add(CaméraJeu);
            Game.Services.AddService(typeof(Caméra), CaméraJeu);
        }
        #endregion

        #region Création Des Menus //juste les nécess?
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
        #endregion

    }
}
