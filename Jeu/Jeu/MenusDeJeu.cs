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

//    enum ÉtatsMenus { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, CHOIX_PROFILE, ENTRÉE_PORT_SERVEUR, ENTRÉE_PORT_CLIENT, MENU_PAUSE, CONNECTION, PAUSE, STAND_BY, ATTENTE_JOUEURS}
//    public class MenusDeJeu : Microsoft.Xna.Framework.GameComponent
//    {
//        ÉtatsJoueur ÉtatJoueur { get; set; }
//        ÉtatsMenus État { get; set; }
//        MenuPrincipal MenuPrincipal { get; set; }
//        MenuOption MenuDesOptions { get; set; }
//        MenuLan MenuNetwork { get; set; }
//        MenuProfile MenuChoixProfile { get; set; }
//        MenuIPServeur MenuServeur { get; set; }
//        MenuIPClient MenuClient { get; set; }
//        MenuPause MenuDePause { get; set; }
//        Menu MenuSélectionnéOption { get; set; }
//        ÉtatsMenus ÉtatPrécédentOption { get; set; }

//        Server Serveur { get; set; }
//        Réseautique NetworkManager { get; set; }
//        List<string> UsedIP { get; set; } //LEGIT?

//        public MenusDeJeu(Game game)
//            : base(game)
//        {

//        }

//        public override void Initialize()
//        {
//            État = ÉtatsMenus.MENU_PRINCIPAL;
//            CréerMenus();
//            base.Initialize();
//        }

//        private void CréerMenus()
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
//            GérerTransition();
//            base.Update(gameTime);
//        }

//        private void GérerTransition()
//        {
//            switch (État)
//            {
//                case ÉtatsMenus.MENU_PRINCIPAL:
//                    GérerTransitionMenuPrincipal();
//                    break;
//                case ÉtatsMenus.MENU_OPTION:
//                    GérerTransitionMenuOption();
//                    break;
//                case ÉtatsMenus.CHOIX_LAN:
//                    GérerTransitionMenuChoixLan();
//                    break;
//                case ÉtatsMenus.ENTRÉE_PORT_SERVEUR:
//                    GérerTransitionMenuServeur();
//                    break;
//                case ÉtatsMenus.ENTRÉE_PORT_CLIENT:
//                    GérerTransitionMenuClient();
//                    break;
//                case ÉtatsMenus.CHOIX_PROFILE:
//                    GérerTransitionMenuProfile();
//                    break;
//                case ÉtatsMenus.CONNECTION:
//                    GérerTransitionConnection();
//                    break;
//                case ÉtatsMenus.ATTENTE_JOUEURS:
//                    GérerTransitionAttenteJoueurs();
//                    break;
//                    //à faire plus tard..
//                //case ÉtatsMenus.PAUSE:
//                //    GérerTransitionPause();
//                //    break;
//                //case ÉtatsMenus.STAND_BY:
//                //    GérerTransitionStandBy();
//                //    break;
//            }
//        }
//        void GérerTransitionMenuPrincipal()
//        {
//            switch (MenuPrincipal.Choix)
//            {
//                case ChoixMenu.EN_ATTENTE:
//                    break;
//                case ChoixMenu.JOUER:
//                    État = ÉtatsMenus.CHOIX_LAN;
//                    MenuPrincipal.Enabled = false;
//                    MenuNetwork.Enabled = true;
//                    break;
//                case ChoixMenu.OPTION:
//                    ÉtatPrécédentOption = État;
//                    État = ÉtatsMenus.MENU_OPTION;
//                    MenuPrincipal.ChangerActivationMenu(false);
//                    MenuDesOptions.Enabled = true;
//                    MenuSélectionnéOption = MenuPrincipal;
//                    break;
//                case ChoixMenu.QUITTER:
//                    Game.Exit();
//                    break;
//            }
//        }
//        void GérerTransitionMenuOption()
//        {
//            switch (MenuDesOptions.Choix)
//            {
//                case ChoixMenu.EN_ATTENTE:
//                    break;
//                case ChoixMenu.RETOUR:
//                    État = ÉtatPrécédentOption;
//                    MenuDesOptions.Enabled = false;
//                    MenuSélectionnéOption.ChangerActivationMenu(true);
//                    break;
//            }
//        }
//        void GérerTransitionMenuChoixLan()
//        {
//            switch (MenuNetwork.Choix)
//            {
//                case ChoixMenu.EN_ATTENTE:
//                    break;
//                case ChoixMenu.SOLO:
//                    État = ÉtatsMenus.CHOIX_PROFILE;
//                    MenuNetwork.Enabled = false;
//                    MenuChoixProfile.Enabled = true;
//                    ÉtatJoueur = ÉtatsJoueur.SOLO;
//                    ConnectionAuServeur("127.0.0.1", 5001); //dans jeu
//                    break;
//                case ChoixMenu.REJOINDRE:
//                    État = ÉtatsMenus.ENTRÉE_PORT_CLIENT;
//                    MenuNetwork.Enabled = false;
//                    MenuClient.Enabled = true;
//                    ÉtatJoueur = ÉtatsJoueur.CLIENT;
//                    break;
//                case ChoixMenu.SERVEUR:
//                    État = ÉtatsMenus.ENTRÉE_PORT_SERVEUR;
//                    MenuNetwork.Enabled = false;
//                    MenuServeur.Enabled = true;
//                    ÉtatJoueur = ÉtatsJoueur.SERVEUR;
//                    break;
//                case ChoixMenu.RETOUR:
//                    État = ÉtatsMenus.MENU_PRINCIPAL;
//                    MenuNetwork.Enabled = false;
//                    MenuPrincipal.Enabled = true;
//                    break;
//            }
//        }
//        void GérerTransitionMenuServeur()
//        {
//            switch (MenuServeur.Choix)
//            {
//                case ChoixMenu.EN_ATTENTE:
//                    break;
//                case ChoixMenu.CONNECTION:
//                    État = ÉtatsMenus.CONNECTION;
//                    ConnectionAuServeur(MenuServeur.IP, MenuServeur.Port); //dans jeu
//                    break;
//                case ChoixMenu.QUITTER:
//                    État = ÉtatsMenus.CHOIX_LAN;
//                    MenuServeur.Enabled = false;
//                    MenuNetwork.Enabled = true;
//                    break;
//            }
//        }
//        private void GérerTransitionMenuClient()
//        {
//            switch (MenuClient.Choix)
//            {

//                case ChoixMenu.EN_ATTENTE:
//                    break;
//                case ChoixMenu.CONNECTION:
//                    État = ÉtatsMenus.CHOIX_PROFILE;
//                    MenuClient.Enabled = false;
//                    MenuChoixProfile.Enabled = true;
//                    ConnectionAuServeur(MenuClient.IP, MenuClient.Port); //DansJeu
//                    break;
//                case ChoixMenu.QUITTER:
//                    État = ÉtatsMenus.CHOIX_LAN;
//                    MenuClient.Enabled = false;
//                    MenuNetwork.Enabled = true;
//                    break;


//        private void GérerTransitionMenuProfile()
//        {
//            switch (MenuChoixProfile.Choix)
//            {
//                case ChoixMenu.EN_ATTENTE:
//                    break;
//                case ChoixMenu.VALIDATION:
//                    if (ÉtatJoueur == ÉtatsJoueur.CLIENT)
//                    {
//                        NetworkManager.SendPrêtJeu(true);
//                    }
//                    État = ÉtatsMenus.ATTENTE_JOUEURS;
//                    break;
//                case ChoixMenu.JOUER:
//                    //retirer tous les menus des components?
//                    État = ÉtatsMenus.DÉCOMPTE;  //dans jeu
//                    MenuChoixProfile.Enabled = false;
//                    InitialiserLeJeu();
//                    InitialiserDécompte();
//                    break;
//            }
//        }


//    }
//}
