//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Media;


//namespace AtelierXNA
//{

//    enum �tatsMenus { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, CHOIX_PROFILE, ENTR�E_PORT_SERVEUR, ENTR�E_PORT_CLIENT, MENU_PAUSE, CONNECTION, PAUSE, STAND_BY, ATTENTE_JOUEURS}
//    public class MenusDeJeu : Microsoft.Xna.Framework.GameComponent
//    {
//        �tatsJoueur �tatJoueur { get; set; }
//        �tatsMenus �tat { get; set; }
//        MenuPrincipal MenuPrincipal { get; set; }
//        MenuOption MenuDesOptions { get; set; }
//        MenuLan MenuNetwork { get; set; }
//        MenuProfile MenuChoixProfile { get; set; }
//        MenuIPServeur MenuServeur { get; set; }
//        MenuIPClient MenuClient { get; set; }
//        MenuPause MenuDePause { get; set; }
//        Menu MenuS�lectionn�Option { get; set; }
//        �tatsMenus �tatPr�c�dentOption { get; set; }

//        Server Serveur { get; set; }
//        R�seautique NetworkManager { get; set; }
//        List<string> UsedIP { get; set; } //LEGIT?

//        public MenusDeJeu(Game game)
//            : base(game)
//        {

//        }

//        public override void Initialize()
//        {
//            �tat = �tatsMenus.MENU_PRINCIPAL;
//            Cr�erMenus();
//            base.Initialize();
//        }

//        private void Cr�erMenus()
//        {
//            MenuPrincipal = new MenuPrincipal(Game);
//            MenuDesOptions = new MenuOption(Game);
//            MenuNetwork = new MenuLan(Game);
//            MenuChoixProfile = new MenuProfile(Game);
//            MenuServeur = new MenuIPServeur(Game);
//            MenuClient = new MenuIPClient(Game);
//            MenuDePause = new MenuPause(Game);

//            Game.Components.Add(MenuPrincipal);
//            Game.Components.Add(MenuDesOptions);
//            Game.Components.Add(MenuNetwork);
//            Game.Components.Add(MenuChoixProfile);
//            Game.Components.Add(MenuServeur);
//            Game.Components.Add(MenuClient);
//            Game.Components.Add(MenuDePause);
//        }

//        public override void Update(GameTime gameTime)
//        {
//            G�rerTransition();
//            base.Update(gameTime);
//        }

//        private void G�rerTransition()
//        {
//            switch (�tat)
//            {
//                case �tatsMenus.MENU_PRINCIPAL:
//                    G�rerTransitionMenuPrincipal();
//                    break;
//                case �tatsMenus.MENU_OPTION:
//                    G�rerTransitionMenuOption();
//                    break;
//                case �tatsMenus.CHOIX_LAN:
//                    G�rerTransitionMenuChoixLan();
//                    break;
//                case �tatsMenus.ENTR�E_PORT_SERVEUR:
//                    G�rerTransitionMenuServeur();
//                    break;
//                case �tatsMenus.ENTR�E_PORT_CLIENT:
//                    G�rerTransitionMenuClient();
//                    break;
//                case �tatsMenus.CHOIX_PROFILE:
//                    G�rerTransitionMenuProfile();
//                    break;
//                case �tatsMenus.CONNECTION:
//                    G�rerTransitionConnection();
//                    break;
//                case �tatsMenus.ATTENTE_JOUEURS:
//                    G�rerTransitionAttenteJoueurs();
//                    break;
//                    //� faire plus tard..
//                //case �tatsMenus.PAUSE:
//                //    G�rerTransitionPause();
//                //    break;
//                //case �tatsMenus.STAND_BY:
//                //    G�rerTransitionStandBy();
//                //    break;
//            }
//        }
//        void G�rerTransitionMenuPrincipal()
//        {
//            switch (MenuPrincipal.Choix)
//            {
//                case ChoixMenu.EN_ATTENTE:
//                    break;
//                case ChoixMenu.JOUER:
//                    �tat = �tatsMenus.CHOIX_LAN;
//                    MenuPrincipal.Enabled = false;
//                    MenuNetwork.Enabled = true;
//                    break;
//                case ChoixMenu.OPTION:
//                    �tatPr�c�dentOption = �tat;
//                    �tat = �tatsMenus.MENU_OPTION;
//                    MenuPrincipal.ChangerActivationMenu(false);
//                    MenuDesOptions.Enabled = true;
//                    MenuS�lectionn�Option = MenuPrincipal;
//                    break;
//                case ChoixMenu.QUITTER:
//                    Game.Exit();
//                    break;
//            }
//        }
//        void G�rerTransitionMenuOption()
//        {
//            switch (MenuDesOptions.Choix)
//            {
//                case ChoixMenu.EN_ATTENTE:
//                    break;
//                case ChoixMenu.RETOUR:
//                    �tat = �tatPr�c�dentOption;
//                    MenuDesOptions.Enabled = false;
//                    MenuS�lectionn�Option.ChangerActivationMenu(true);
//                    break;
//            }
//        }
//        void G�rerTransitionMenuChoixLan()
//        {
//            switch (MenuNetwork.Choix)
//            {
//                case ChoixMenu.EN_ATTENTE:
//                    break;
//                case ChoixMenu.SOLO:
//                    �tat = �tatsMenus.CHOIX_PROFILE;
//                    MenuNetwork.Enabled = false;
//                    MenuChoixProfile.Enabled = true;
//                    �tatJoueur = �tatsJoueur.SOLO;
//                    ConnectionAuServeur("127.0.0.1", 5001); //dans jeu
//                    break;
//                case ChoixMenu.REJOINDRE:
//                    �tat = �tatsMenus.ENTR�E_PORT_CLIENT;
//                    MenuNetwork.Enabled = false;
//                    MenuClient.Enabled = true;
//                    �tatJoueur = �tatsJoueur.CLIENT;
//                    break;
//                case ChoixMenu.SERVEUR:
//                    �tat = �tatsMenus.ENTR�E_PORT_SERVEUR;
//                    MenuNetwork.Enabled = false;
//                    MenuServeur.Enabled = true;
//                    �tatJoueur = �tatsJoueur.SERVEUR;
//                    break;
//                case ChoixMenu.RETOUR:
//                    �tat = �tatsMenus.MENU_PRINCIPAL;
//                    MenuNetwork.Enabled = false;
//                    MenuPrincipal.Enabled = true;
//                    break;
//            }
//        }
//        void G�rerTransitionMenuServeur()
//        {
//            switch (MenuServeur.Choix)
//            {
//                case ChoixMenu.EN_ATTENTE:
//                    break;
//                case ChoixMenu.CONNECTION:
//                    �tat = �tatsMenus.CONNECTION;
//                    ConnectionAuServeur(MenuServeur.IP, MenuServeur.Port); //dans jeu
//                    break;
//                case ChoixMenu.QUITTER:
//                    �tat = �tatsMenus.CHOIX_LAN;
//                    MenuServeur.Enabled = false;
//                    MenuNetwork.Enabled = true;
//                    break;
//            }
//        }
//        private void G�rerTransitionMenuClient()
//        {
//            switch (MenuClient.Choix)
//            {

//                case ChoixMenu.EN_ATTENTE:
//                    break;
//                case ChoixMenu.CONNECTION:
//                    �tat = �tatsMenus.CHOIX_PROFILE;
//                    MenuClient.Enabled = false;
//                    MenuChoixProfile.Enabled = true;
//                    ConnectionAuServeur(MenuClient.IP, MenuClient.Port); //DansJeu
//                    break;
//                case ChoixMenu.QUITTER:
//                    �tat = �tatsMenus.CHOIX_LAN;
//                    MenuClient.Enabled = false;
//                    MenuNetwork.Enabled = true;
//                    break;


//        private void G�rerTransitionMenuProfile()
//        {
//            switch (MenuChoixProfile.Choix)
//            {
//                case ChoixMenu.EN_ATTENTE:
//                    break;
//                case ChoixMenu.VALIDATION:
//                    if (�tatJoueur == �tatsJoueur.CLIENT)
//                    {
//                        NetworkManager.SendPr�tJeu(true);
//                    }
//                    �tat = �tatsMenus.ATTENTE_JOUEURS;
//                    break;
//                case ChoixMenu.JOUER:
//                    //retirer tous les menus des components?
//                    �tat = �tatsMenus.D�COMPTE;  //dans jeu
//                    MenuChoixProfile.Enabled = false;
//                    InitialiserLeJeu();
//                    InitialiserD�compte();
//                    break;
//            }
//        }


//    }
//}
