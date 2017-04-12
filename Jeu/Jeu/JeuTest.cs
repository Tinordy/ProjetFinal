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


namespace AtelierXNA
{
    enum ÉtatsJeuT { /*MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, */JEU, /*CHOIX_PROFILE, *//*ENTRÉE_PORT_SERVEUR, ENTRÉE_PORT_CLIENT, */MENU_PAUSE, /*CONNECTION, *//*ATTENTE_JOUEURS, */DÉCOMPTE, PAUSE, STAND_BY, GAGNÉ, PERDU, FIN_DE_PARTIE, MENU }

    public class JeuTest : Microsoft.Xna.Framework.GameComponent
    {
        const int ÉTENDUE = 50;
        const float INTERVALLE_MAJ = 1f / 60f;
        List<string> UsedIP { get; set; } //LEGIT?
        bool pause;
        bool Pause
        {
            get
            {
                return pause;
            }
            set
            {
                pause = value;
                Joueur.EstActif = !pause;
                NetworkManager.TempsDeCourseJ.EstActif = !pause;
                NetworkManager.SendPrêtJeu(!pause);
                //ARRÊter TOUTES LES VOITURE? juste voitures robots + objets?

            }
        }
        bool gagné;
        bool Gagné
        {
            get
            {
                return gagné;
            }
            set
            {
                gagné = value;
                NetworkManager.SendTerminé(gagné, NetworkManager.TempsDeCourseJ.ValeurTimer, NetworkManager.PseudonymeJ);
                if (gagné)
                {
                    État = ÉtatsJeuT.GAGNÉ;
                    NetworkManager.TempsDeCourseJ.EstActif = false;
                }
                else
                {
                    État = ÉtatsJeuT.PERDU;
                }
                //différent, fin de partie
                //Game.Components.Add(new Titre(Game, État.ToString(), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2), "Blanc"));
            }
        }
        bool JoueurEstArrivé
        {
            get
            {
                //modifier
                return 492 < Joueur.Position.X && 508 > Joueur.Position.X && 392 < Joueur.Position.Z && 408 > Joueur.Position.Z;
            }
        }
        ÉtatsJeuT État { get; set; }
        ÉtatsJoueur ÉtatJoueur { get; set; }
        ÉtatsMenu ÉtatMenu { get; set; }
        MenuPrincipal MenuPrincipal { get; set; }
        MenuOption MenuDesOptions { get; set; }
        MenuLan MenuNetwork { get; set; }
        MenuProfile MenuChoixProfile { get; set; }
        MenuIPServeur MenuServeur { get; set; }
        MenuIPClient MenuClient { get; set; }
        MenuPause MenuDePause { get; set; }
        MenuFinPartie MenuFinDePartie { get; set; }
        Menu MenuSélectionnéOption { get; set; }
        ÉtatsMenu ÉtatPrécédentOption { get; set; }

        Server Serveur { get; set; }
        Réseautique NetworkManager { get; set; }
        Voiture Joueur { get; set; }
        Voiture Ennemi { get; set; }
        List<Section> Sections { get; set; }
        TimerDiminue DécompteInitial { get; set; }
        //TimerAugmente TempsDeCourse { get; set; }
        Vector2 ÉtendueTotale { get; set; }
        InputManager GestionInput { get; set; }
        public JeuTest(Game game)
            : base(game)
        {
            CréerMenus();
        }
        public override void Initialize()
        {
            ÉtatJoueur = ÉtatsJoueur.SOLO;
            CréerCaméra();
            CréerEnvironnement();
            Game.Components.Add(new AfficheurFPS(Game, "Arial20", Color.White, 1f));

            UsedIP = new List<string>();
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            État = ÉtatsJeuT.MENU;
            ÉtatMenu = ÉtatsMenu.MENU_PRINCIPAL;
            //MenuPrincipal.Enabled = true;
        }

        public override void Update(GameTime gameTime)
        {
            GérerTransition();
            GérerÉtat();
            GérerDéconnection();
        }

        private void GérerDéconnection()
        {
            if (NetworkManager != null && NetworkManager.DisconnectedT)
            {
                État = ÉtatsJeuT.MENU;
                ÉtatMenu = ÉtatsMenu.MENU_PRINCIPAL;
                MenuPrincipal.Enabled = true;
                //désactiver tous les menus
                MenuDePause.Enabled = false;
                MenuFinDePartie.Enabled = false;
                NetworkManager.DisconnectedT = false;
            }
        }

        private void GérerÉtat()
        {
            switch (État)
            {
                case ÉtatsJeuT.JEU:
                    GérerÉtatJeu();
                    break;
            }
        }

        private void GérerÉtatJeu()
        {
            //if player moved?
            if (ÉtatJoueur != ÉtatsJoueur.SOLO /*&& NetworkManager.EnnemiPrêtÀJouer*/) //TEST
            {
                Ennemi.AjusterPosition(NetworkManager.MatriceMondeEnnemi);
                GérerCollisions();
            }
            //Collision, nb tours?
        }

        private void GérerCollisions()
        {
            //collision entre joueurs
            if (Joueur.EstEnCollision(Ennemi))
            {
                Joueur.Rebondir();
                Ennemi.Rebondir();
            }
        }



        #region Transitions
        void GérerTransition()
        {
            switch (État)
            {
                case ÉtatsJeuT.MENU:
                    GérerTransitionMenus();
                    break;
                //case ÉtatsJeu.MENU_PRINCIPAL:
                //    GérerTransitionMenuPrincipal();
                //    break;
                //case ÉtatsJeu.MENU_OPTION:
                //    GérerTransitionMenuOption();
                //    break;
                //case ÉtatsJeu.CHOIX_LAN:
                //    GérerTransitionMenuChoixLan();
                //    break;
                //case ÉtatsJeu.CHOIX_PROFILE:
                //    GérerTransitionMenuProfile();
                //    break;
                //case ÉtatsJeu.ENTRÉE_PORT_SERVEUR:
                //    GérerTransitionMenuServeur();
                //    break;
                //case ÉtatsJeu.ENTRÉE_PORT_CLIENT:
                //    GérerTransitionMenuClient();
                //    break;
                //case ÉtatsJeu.CONNECTION:
                //    GérerTransitionConnection();
                //    break;
                //case ÉtatsJeu.ATTENTE_JOUEURS:
                //    GérerTransitionAttenteJoueurs();
                //    break;
                case ÉtatsJeuT.DÉCOMPTE:
                    GérerTransitionDécompte();
                    break;
                case ÉtatsJeuT.JEU:
                    GérerTransitionJeu();
                    break;
                case ÉtatsJeuT.PAUSE:
                    GérerTransitionPause();
                    break;
                case ÉtatsJeuT.STAND_BY:
                    GérerTransitionStandBy();
                    break;
                case ÉtatsJeuT.GAGNÉ:
                    GérerTransitionGagné();
                    break;
                case ÉtatsJeuT.PERDU:
                    GérerTransitionPerdu();
                    break;
                case ÉtatsJeuT.FIN_DE_PARTIE:
                    GérerTransitionFinDePartie();
                    break;
            }

        }

        private void GérerTransitionMenus()
        {
            switch(ÉtatMenu)
            {
                case ÉtatsMenu.MENU_PRINCIPAL:
                    GérerTransitionMenuPrincipal();
                    break;
                case ÉtatsMenu.MENU_OPTION:
                    GérerTransitionMenuOption();
                    break;
                case ÉtatsMenu.CHOIX_LAN:
                    GérerTransitionMenuChoixLan();
                    break;
                case ÉtatsMenu.CHOIX_PROFILE:
                    GérerTransitionMenuProfile();
                    break;
                case ÉtatsMenu.ENTRÉE_PORT_SERVEUR:
                    GérerTransitionMenuServeur();
                    break;
                case ÉtatsMenu.ENTRÉE_PORT_CLIENT:
                    GérerTransitionMenuClient();
                    break;
                case ÉtatsMenu.ATTENTE_JOUEURS:
                    GérerTransitionAttenteJoueurs();
                    break;
                case ÉtatsMenu.CONNECTION:
                    GérerTransitionConnection();
                    break;
            }
        }

        private void GérerTransitionFinDePartie()
        {
            switch (MenuFinDePartie.Choix)
            {
                case ChoixMenu.JOUER:
                    MenuFinDePartie.Enabled = false;
                    MenuChoixProfile.Enabled = true;
                    État = ÉtatsJeuT.MENU;
                    ÉtatMenu = ÉtatsMenu.CHOIX_PROFILE;
                    //Envoyer client!!! REJOUER!!!
                    break;
                case ChoixMenu.QUITTER:
                    État = ÉtatsJeuT.MENU;
                    ÉtatMenu = ÉtatsMenu.MENU_PRINCIPAL;
                    MenuFinDePartie.Enabled = false;
                    MenuPrincipal.Enabled = true;
                    NetworkManager.SendDisconnect();
                    break;
            }
        }

        private void GérerTransitionPerdu()
        {
            if (JoueurEstArrivé)
            {
                État = ÉtatsJeuT.FIN_DE_PARTIE;
                MenuFinDePartie.Enabled = true;
                NetworkManager.TempsDeCourseJ.EstActif = false;
                Joueur.EstActif = false;
                NetworkManager.SendTerminé(true, NetworkManager.TempsDeCourseJ.ValeurTimer, NetworkManager.PseudonymeJ);
            }
            else
            {
                Ennemi.AjusterPosition(NetworkManager.MatriceMondeEnnemi);
            }
        }

        private void GérerTransitionGagné()
        {
            if (NetworkManager.EnnemiEstArrivé || ÉtatJoueur == ÉtatsJoueur.SOLO)
            {
                État = ÉtatsJeuT.FIN_DE_PARTIE;
                MenuFinDePartie.Enabled = true;
                Joueur.Enabled = false;
            }
            else
            {
                Ennemi.AjusterPosition(NetworkManager.MatriceMondeEnnemi);
            }
        }

        private void GérerTransitionStandBy()
        {
            if (NetworkManager.EnnemiPrêtÀJouer)
            {
                État = ÉtatsJeuT.JEU;
                Pause = false;
                MenuDePause.Enabled = false;
            }
        }

        private void GérerTransitionPause()
        {
            switch (MenuDePause.Choix)
            {
                case ChoixMenu.JOUER:
                    MenuDePause.Enabled = false;
                    État = ÉtatsJeuT.JEU;
                    Pause = false; //AJOUTER DÉCOMPTE :(
                    break;
                case ChoixMenu.QUITTER:
                    MenuDePause.Enabled = false;
                    État = ÉtatsJeuT.MENU;
                    ÉtatMenu = ÉtatsMenu.MENU_PRINCIPAL;
                    MenuPrincipal.Enabled = true;
                    NetworkManager.SendDisconnect();
                    break;
                case ChoixMenu.OPTION:
                    ÉtatPrécédentOption = ÉtatMenu; //NOT WORKING... PAUSE != menu! yet
                    État = ÉtatsJeuT.MENU;
                    ÉtatMenu = ÉtatsMenu.MENU_OPTION;
                    MenuDesOptions.Enabled = true;
                    MenuDePause.ChangerActivationMenu(false);
                    MenuSélectionnéOption = MenuDePause;
                    break;
            }
        }

        private void GérerTransitionJeu()
        {
            GérerChangementPause();
            GérerGagnant();
        }

        private void GérerGagnant()
        {
            if (NetworkManager.EnnemiEstArrivé)
            {
                Gagné = false;
            }
            else
            {
                if (JoueurEstArrivé)
                {
                    if (ÉtatJoueur == ÉtatsJoueur.SOLO)
                    {
                        Gagné = NetworkManager.TempsDeCourseJ.ValeurTimer < new TimeSpan(0, 0, 10); //obtenir vraie valeur
                    }
                    else
                    {
                        Gagné = true;
                    }
                }
            }
        }


        private void GérerChangementPause()
        {
            if (ÉtatJoueur != ÉtatsJoueur.CLIENT)
            {
                if (GestionInput.EstNouvelleTouche(Keys.Space))
                {
                    État = ÉtatsJeuT.PAUSE;
                    Pause = true;
                    MenuDePause.Enabled = true;
                    MenuDePause.ChangerActivationMenu(true);
                }
            }
            else
            {
                if (!NetworkManager.EnnemiPrêtÀJouer)
                {
                    État = ÉtatsJeuT.STAND_BY;
                    MenuDePause.Enabled = true;
                    MenuDePause.ChangerActivationMenu(false);
                    Pause = true;
                }
            }

        }
        private void GérerTransitionDécompte()
        {
            if (!DécompteInitial.EstActif)
            {
                État = ÉtatsJeuT.JEU;
                Joueur.EstActif = true;
                DécompteInitial.Enabled = false;
                DécompteInitial.Visible = false;
                NetworkManager.TempsDeCourseJ = new TimerAugmente(Game, new TimeSpan(0), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, 30), "Blanc", true, 0.01f);
                Game.Components.Add(NetworkManager.TempsDeCourseJ);
            }
        }

        private void GérerTransitionAttenteJoueurs()
        {
            switch (ÉtatJoueur)
            {
                case ÉtatsJoueur.SOLO:
                    MenuChoixProfile.ActiverBtnDémarrer();
                    ÉtatMenu = ÉtatsMenu.CHOIX_PROFILE;
                    break;
                case ÉtatsJoueur.CLIENT:
                    if (NetworkManager.EnnemiPrêtÀJouer)
                    {
                        MenuChoixProfile.Enabled = false;
                        InitialiserLeJeu();
                        État = ÉtatsJeuT.DÉCOMPTE;
                        InitialiserDécompte();
                    }
                    break;
                case ÉtatsJoueur.SERVEUR:
                    if (NetworkManager.EnnemiPrêtÀJouer)
                    {
                        ÉtatMenu = ÉtatsMenu.CHOIX_PROFILE;
                        MenuChoixProfile.ActiverBtnDémarrer();
                    }
                    break;
            }
        }





        //NOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOON! Trouver solution..?
        //bool t { get; set; }
        private void GérerTransitionConnection()
        {
            if (/*Serveur.connectedClients == 2*/NetworkManager.enemiConnecté)
            {
                //t = true;
                //bool test = NetworkManager.enemiConnecté;
                ÉtatMenu = ÉtatsMenu.CHOIX_PROFILE;
                MenuServeur.Enabled = false;
                MenuChoixProfile.Enabled = true;
            }
        }

        private void GérerTransitionMenuProfile()
        {
            //bool k = NetworkManager.enemiConnecté;
            //t = true;
            switch (MenuChoixProfile.Choix)
            {
                case ChoixMenu.EN_ATTENTE:
                    break;
                case ChoixMenu.VALIDATION:
                    if (ÉtatJoueur == ÉtatsJoueur.CLIENT)
                    {
                        NetworkManager.SendPrêtJeu(true);
                    }
                    ÉtatMenu = ÉtatsMenu.ATTENTE_JOUEURS;
                    NetworkManager.PseudonymeJ = MenuChoixProfile.Pseudonyme;
                    //ENVOYER PSUDONYME A ENNEMI!!!!!!!
                    break;
                case ChoixMenu.JOUER:
                    //retirer tous les menus des components?
                    État = ÉtatsJeuT.DÉCOMPTE;
                    MenuChoixProfile.Enabled = false;
                    InitialiserLeJeu();
                    InitialiserDécompte();
                    break;
            }
        }

        private void InitialiserDécompte()
        {
            DécompteInitial = new TimerDiminue(Game, new TimeSpan(0, 0, 5), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2), "Blanc", true, 1f);
            Game.Components.Add(DécompteInitial);
        }

        void GérerTransitionMenuPrincipal()
        {
            switch (MenuPrincipal.Choix)
            {
                case ChoixMenu.EN_ATTENTE:
                    break;
                case ChoixMenu.JOUER:
                    ÉtatMenu = ÉtatsMenu.CHOIX_LAN;
                    MenuPrincipal.Enabled = false;
                    MenuNetwork.Enabled = true;
                    break;
                case ChoixMenu.OPTION:
                    ÉtatPrécédentOption = ÉtatMenu;
                    ÉtatMenu = ÉtatsMenu.MENU_OPTION;
                    MenuPrincipal.ChangerActivationMenu(false);
                    MenuDesOptions.Enabled = true;
                    MenuSélectionnéOption = MenuPrincipal;
                    break;
                case ChoixMenu.QUITTER:
                    Game.Exit();
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
                    ÉtatMenu = ÉtatPrécédentOption;
                    MenuDesOptions.Enabled = false;
                    MenuSélectionnéOption.ChangerActivationMenu(true);
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
                    ÉtatMenu = ÉtatsMenu.CHOIX_PROFILE;
                    MenuNetwork.Enabled = false;
                    MenuChoixProfile.Enabled = true;
                    ÉtatJoueur = ÉtatsJoueur.SOLO;
                    ConnectionAuServeur("127.0.0.1", 5001);
                    break;
                case ChoixMenu.REJOINDRE:
                    ÉtatMenu = ÉtatsMenu.ENTRÉE_PORT_CLIENT;
                    MenuNetwork.Enabled = false;
                    MenuClient.Enabled = true;
                    ÉtatJoueur = ÉtatsJoueur.CLIENT;
                    break;
                case ChoixMenu.SERVEUR:
                    ÉtatMenu = ÉtatsMenu.ENTRÉE_PORT_SERVEUR;
                    MenuNetwork.Enabled = false;
                    MenuServeur.Enabled = true;
                    ÉtatJoueur = ÉtatsJoueur.SERVEUR;
                    break;
                case ChoixMenu.RETOUR:
                    ÉtatMenu = ÉtatsMenu.MENU_PRINCIPAL;
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
                    ÉtatMenu = ÉtatsMenu.CONNECTION;
                    ConnectionAuServeur(MenuServeur.IP, MenuServeur.Port);
                    break;
                case ChoixMenu.QUITTER:
                    ÉtatMenu = ÉtatsMenu.CHOIX_LAN;
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
                    ÉtatMenu = ÉtatsMenu.CHOIX_PROFILE;
                    MenuClient.Enabled = false;
                    MenuChoixProfile.Enabled = true;
                    ConnectionAuServeur(MenuClient.IP, MenuClient.Port);
                    break;
                case ChoixMenu.QUITTER:
                    ÉtatMenu = ÉtatsMenu.CHOIX_LAN;
                    MenuClient.Enabled = false;
                    MenuNetwork.Enabled = true;
                    break;
            }
        }
        #endregion
        private void ConnectionAuServeur(string ip, int port)
        {
            if (UsedIP.FindIndex(s => s == ip) == -1) //marche simple joueur, pt pas multijoueur?
            {
                Serveur = new Server(port, ip);
                Game.Services.AddService(typeof(Server), Serveur);
                NetworkManager = new Réseautique(Game,/*Serveur,*/ ip, port);
                Game.Services.AddService(typeof(Réseautique), NetworkManager);
                UsedIP.Add(ip);
            }
        }
        #region initialisation du jeu
        private void InitialiserLeJeu()
        {
            Reset();
            CréerVoitureDummy();
            CréerJoueur();
            if (ÉtatJoueur != ÉtatsJoueur.SOLO)
            {
                CréerEnnemi();
            }
        }

        private void CréerVoitureDummy()
        {
            Game.Components.Add(new VoitureDummy(Game, "small car", 0.1f, Vector3.Zero, new Vector3(50, 5, 50), 0.01f));
        }

        private void Reset()
        {
            for (int i = Game.Components.Count - 1; i >= 0; --i)
            {
                if (Game.Components[i] is IResettable)
                {
                    Game.Components.RemoveAt(i);
                }
            }
        }

        private void CréerEnvironnement()
        {
            Game.Components.Add(new ArrièrePlanDéroulant(Game, "CielÉtoilé", 0.01f));
            Sections = new List<Section>();

            ÉtendueTotale = new Vector2(200 * 4, 200 * 4); //envoyer à voiture?
            for (int i = 0; i < 10; ++i)
            {
                for (int j = 0; j < 10; ++j)
                {
                    Section newSection = new Section(Game, new Vector2(ÉTENDUE * i, ÉTENDUE * j), new Vector2(ÉTENDUE, ÉTENDUE), 1f, Vector3.Zero, Vector3.Zero, new Vector3(ÉTENDUE, 25, ÉTENDUE), new string[] { "Herbe", "Sable" }, 0.01f); //double??
                    Sections.Add(newSection);
                    Game.Components.Add(newSection);
                    newSection.DrawOrder = 0; //le terrain doit être dessiné en 2e
                }
            }
            Game.Components.Add(new Maison(Game, 10f, Vector3.Zero, new Vector3(500, 0, 400), new Vector3(2, 2, 2), "Carte", "BoutonVert", 0.01f));

        }

        private void CréerEnnemi()
        {
            //Vrai Posinitiale :(
            Ennemi = new Voiture(Game, "GLX_400", 0.01f, Vector3.Zero, NetworkManager.PositionEnnemi, 0.01f); //Get choix de voiture??
            Ennemi.EstActif = false;
            Game.Components.Add(Ennemi);
        }

        private void CréerJoueur()
        {
            Joueur = new Voiture(Game, "GLX_400", 0.01f, Vector3.Zero, new Vector3(100, 0, 50), 0.01f); //mettre choix?
            Joueur.EstActif = false;
            Game.Components.Add(Joueur);
            NetworkManager.SendPosIni(Joueur.Position); //fonctionne pas....
        }

        private void CréerCaméra()
        {
            Vector3 positionCaméra = new Vector3(100, 50, 125);
            Vector3 cibleCaméra = new Vector3(100, 0, 50);
            CaméraSubjective CaméraJeu = new CaméraSubjective(Game, positionCaméra, cibleCaméra, Vector3.Up, 0.01f);
            Game.Components.Add(CaméraJeu);
            Game.Services.AddService(typeof(Caméra), CaméraJeu);
        }
        #endregion

        #region Création Des Menus
        void CréerMenus()
        {
            //faire une liste pi add?
            MenuPrincipal = new MenuPrincipal(Game);
            Game.Components.Add(MenuPrincipal);
            MenuDePause = new MenuPause(Game);
            Game.Components.Add(MenuDePause);
            MenuFinDePartie = new MenuFinPartie(Game);
            Game.Components.Add(MenuFinDePartie);
            MenuDesOptions = new MenuOption(Game);
            Game.Components.Add(MenuDesOptions);
            MenuNetwork = new MenuLan(Game);
            Game.Components.Add(MenuNetwork);
            MenuChoixProfile = new MenuProfile(Game);
            Game.Components.Add(MenuChoixProfile);
            MenuServeur = new MenuIPServeur(Game);
            Game.Components.Add(MenuServeur);
            MenuClient = new MenuIPClient(Game);
            Game.Components.Add(MenuClient);
        }
        #endregion
    }
}
