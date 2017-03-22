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
        //const int BUFFER_SIZE = 2048;
        �tatsJeu �tat { get; set; }
        �tatsJoueur �tatJoueur { get; set; }
        MenuPrincipal MenuPrincipal { get; set; }
        MenuOption MenuDesOptions { get; set; }
        MenuLan MenuNetwork { get; set; }
        MenuProfile MenuChoixProfile { get; set; }
        MenuIPServeur MenuServeur { get; set; }
        MenuIPClient MenuClient { get; set; }

        Server Serveur { get; set; }
        R�seautique NetworkManager { get; set; }
        Voiture Joueur { get; set; }
        Voiture Ennemi { get; set; }
        List<Section> Sections { get; set; }

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
            G�rer�tat();
        }

        private void G�rer�tat()
        {
            switch (�tat)
            {
                case �tatsJeu.JEU:
                    //if player moved?
                    if(�tatJoueur != �tatsJoueur.SOLO)
                    {
                        Ennemi.AjusterPosition(NetworkManager.MatriceMondeEnnemi);
                    }

                    //Collision, gagnant..???
                    break;
            }


        }
        #region Transitions
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
                    if (NetworkManager.EnnemiPr�t�Jouer)
                    {
                        //INITIALISATION?
                        MenuChoixProfile.Enabled = false;
                        D�marrerLeJeu();
                        �tat = �tatsJeu.JEU;
                    }
                    break;
                case �tatsJoueur.SERVEUR:
                    if (NetworkManager.EnnemiPr�t�Jouer)
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
                        NetworkManager.writeStream.Position = 0;
                        NetworkManager.writer.Write((byte)Protocoles.ReadyToPlayChanged);
                        NetworkManager.writer.Write(true);
                        NetworkManager.SendData(Serveur.GetDataFromMemoryStream(NetworkManager.writeStream));
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
                case ChoixMenu.QUITTER:
                    �tat = �tatsJeu.CHOIX_LAN;
                    MenuServeur.Enabled = false;
                    MenuNetwork.Enabled = true;
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
                case ChoixMenu.QUITTER:
                    �tat = �tatsJeu.CHOIX_LAN;
                    MenuClient.Enabled = false;
                    MenuNetwork.Enabled = true;
                    break;
            }
        }
        #endregion
        private void ConnectionAuServeur(string ip, int port)
        {
            Serveur = new Server(port, ip);
            Game.Services.AddService(typeof(Server), Serveur); //n�cessaire?
            NetworkManager = new R�seautique(Serveur, ip, port);
            Game.Services.AddService(typeof(R�seautique), NetworkManager);

        }
        #region initialisation du jeu
        private void D�marrerLeJeu()
        {
            //changer de cam�ra?
            Cr�erCam�ra();
            Cr�erJoueur();
            if(�tatJoueur != �tatsJoueur.SOLO)
            {
                Cr�erEnnemi();
            }
            Cr�erEnvironnement();
        }

        private void Cr�erEnvironnement()
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

        private void Cr�erEnnemi()
        {
            Ennemi = new Voiture(Game, "unicorn", 20f, Vector3.Zero, NetworkManager.PositionEnnemi, 1.01f); //Get choix de voiture??
            Game.Components.Add(Ennemi);
        }

        private void Cr�erJoueur()
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

        private void Cr�erCam�ra()
        {
            Vector3 positionCam�ra = new Vector3(200, 10, 200);
            Vector3 cibleCam�ra = new Vector3(10, 0, 10);
            Cam�raSubjective Cam�raJeu = new Cam�raSubjective(Game, positionCam�ra, cibleCam�ra, Vector3.Up, 0.01f);
            Game.Components.Add(Cam�raJeu);
            Game.Services.AddService(typeof(Cam�ra), Cam�raJeu);
        }
        #endregion

        #region Cr�ation Des Menus //juste les n�cess?
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
        #endregion

    }
}
