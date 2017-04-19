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
    enum �tatsJeuT { /*MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, */JEU, /*CHOIX_PROFILE, *//*ENTR�E_PORT_SERVEUR, ENTR�E_PORT_CLIENT, */MENU_PAUSE, /*CONNECTION, *//*ATTENTE_JOUEURS, */D�COMPTE, PAUSE, STAND_BY, GAGN�, PERDU, FIN_DE_PARTIE, MENU }

    public class JeuTest : Microsoft.Xna.Framework.GameComponent
    {
        const int �TENDUE = 50;
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
                NetworkManager.SendPr�tJeu(!pause);
                //ARR�ter TOUTES LES VOITURE? juste voitures robots + objets?

            }
        }
        bool gagn�;
        bool Gagn�
        {
            get
            {
                return gagn�;
            }
            set
            {
                gagn� = value;
                NetworkManager.SendTermin�(gagn�, NetworkManager.TempsDeCourseJ.ValeurTimer, NetworkManager.PseudonymeJ);
                if (gagn�)
                {
                    �tat = �tatsJeuT.GAGN�;
                    NetworkManager.TempsDeCourseJ.EstActif = false;
                }
                else
                {
                    �tat = �tatsJeuT.PERDU;
                }
                //diff�rent, fin de partie
                //Game.Components.Add(new Titre(Game, �tat.ToString(), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2), "Blanc"));
            }
        }
        bool JoueurEstArriv�
        {
            get
            {
                //modifier
                return 492 < Joueur.Position.X && 508 > Joueur.Position.X && 392 < Joueur.Position.Z && 408 > Joueur.Position.Z;
            }
        }
        �tatsJeuT �tat { get; set; }
        �tatsJoueur �tatJoueur { get; set; }
        �tatsMenu �tatMenu { get; set; }
        MenuPrincipal MenuPrincipal { get; set; }
        MenuOption MenuDesOptions { get; set; }
        MenuLan MenuNetwork { get; set; }
        MenuProfile MenuChoixProfile { get; set; }
        MenuIPServeur MenuServeur { get; set; }
        MenuIPClient MenuClient { get; set; }
        MenuPause MenuDePause { get; set; }
        MenuFinPartie MenuFinDePartie { get; set; }
        Menu MenuS�lectionn�Option { get; set; }
        �tatsMenu �tatPr�c�dentOption { get; set; }

        Server Serveur { get; set; }
        R�seautique NetworkManager { get; set; }
        Voiture Joueur { get; set; }
        Voiture Ennemi { get; set; }
        List<Section> Sections { get; set; }
        TimerDiminue D�compteInitial { get; set; }
        //TimerAugmente TempsDeCourse { get; set; }
        Vector2 �tendueTotale { get; set; }
        InputManager GestionInput { get; set; }
        public JeuTest(Game game)
            : base(game)
        {
            Cr�erMenus();
        }
        public override void Initialize()
        {
            �tatJoueur = �tatsJoueur.SOLO;
            Cr�erCam�ra();
            Cr�erEnvironnement();
            Game.Components.Add(new AfficheurFPS(Game, "Arial20", Color.White, 1f));

            UsedIP = new List<string>();
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            �tat = �tatsJeuT.MENU;
            �tatMenu = �tatsMenu.MENU_PRINCIPAL;
            //MenuPrincipal.Enabled = true;
        }

        public override void Update(GameTime gameTime)
        {
            G�rerTransition();
            G�rer�tat();
            G�rerD�connection();
        }

        private void G�rerD�connection()
        {
            if (NetworkManager != null && NetworkManager.DisconnectedT)
            {
                �tat = �tatsJeuT.MENU;
                �tatMenu = �tatsMenu.MENU_PRINCIPAL;
                MenuPrincipal.Enabled = true;
                //d�sactiver tous les menus
                MenuDePause.Enabled = false;
                MenuFinDePartie.Enabled = false;
                NetworkManager.DisconnectedT = false;
            }
        }

        private void G�rer�tat()
        {
            switch (�tat)
            {
                case �tatsJeuT.JEU:
                    G�rer�tatJeu();
                    break;
            }
        }

        private void G�rer�tatJeu()
        {
            //if player moved?
            if (�tatJoueur != �tatsJoueur.SOLO /*&& NetworkManager.EnnemiPr�t�Jouer*/) //TEST
            {
                Ennemi.AjusterPosition(NetworkManager.MatriceMondeEnnemi);
                G�rerCollisions();
            }
            //Collision, nb tours?
        }

        private void G�rerCollisions()
        {
            //collision entre joueurs
            if (Joueur.EstEnCollision(Ennemi))
            {
                Joueur.Rebondir();
                Ennemi.Rebondir();
            }
        }



        #region Transitions
        void G�rerTransition()
        {
            switch (�tat)
            {
                case �tatsJeuT.MENU:
                    G�rerTransitionMenus();
                    break;
                //case �tatsJeu.MENU_PRINCIPAL:
                //    G�rerTransitionMenuPrincipal();
                //    break;
                //case �tatsJeu.MENU_OPTION:
                //    G�rerTransitionMenuOption();
                //    break;
                //case �tatsJeu.CHOIX_LAN:
                //    G�rerTransitionMenuChoixLan();
                //    break;
                //case �tatsJeu.CHOIX_PROFILE:
                //    G�rerTransitionMenuProfile();
                //    break;
                //case �tatsJeu.ENTR�E_PORT_SERVEUR:
                //    G�rerTransitionMenuServeur();
                //    break;
                //case �tatsJeu.ENTR�E_PORT_CLIENT:
                //    G�rerTransitionMenuClient();
                //    break;
                //case �tatsJeu.CONNECTION:
                //    G�rerTransitionConnection();
                //    break;
                //case �tatsJeu.ATTENTE_JOUEURS:
                //    G�rerTransitionAttenteJoueurs();
                //    break;
                case �tatsJeuT.D�COMPTE:
                    G�rerTransitionD�compte();
                    break;
                case �tatsJeuT.JEU:
                    G�rerTransitionJeu();
                    break;
                case �tatsJeuT.PAUSE:
                    G�rerTransitionPause();
                    break;
                case �tatsJeuT.STAND_BY:
                    G�rerTransitionStandBy();
                    break;
                case �tatsJeuT.GAGN�:
                    G�rerTransitionGagn�();
                    break;
                case �tatsJeuT.PERDU:
                    G�rerTransitionPerdu();
                    break;
                case �tatsJeuT.FIN_DE_PARTIE:
                    G�rerTransitionFinDePartie();
                    break;
            }

        }

        private void G�rerTransitionMenus()
        {
            switch(�tatMenu)
            {
                case �tatsMenu.MENU_PRINCIPAL:
                    G�rerTransitionMenuPrincipal();
                    break;
                case �tatsMenu.MENU_OPTION:
                    G�rerTransitionMenuOption();
                    break;
                case �tatsMenu.CHOIX_LAN:
                    G�rerTransitionMenuChoixLan();
                    break;
                case �tatsMenu.CHOIX_PROFILE:
                    G�rerTransitionMenuProfile();
                    break;
                case �tatsMenu.ENTR�E_PORT_SERVEUR:
                    G�rerTransitionMenuServeur();
                    break;
                case �tatsMenu.ENTR�E_PORT_CLIENT:
                    G�rerTransitionMenuClient();
                    break;
                case �tatsMenu.ATTENTE_JOUEURS:
                    G�rerTransitionAttenteJoueurs();
                    break;
                case �tatsMenu.CONNECTION:
                    G�rerTransitionConnection();
                    break;
            }
        }

        private void G�rerTransitionFinDePartie()
        {
            switch (MenuFinDePartie.Choix)
            {
                case ChoixMenu.JOUER:
                    MenuFinDePartie.Enabled = false;
                    MenuChoixProfile.Enabled = true;
                    �tat = �tatsJeuT.MENU;
                    �tatMenu = �tatsMenu.CHOIX_PROFILE;
                    //Envoyer client!!! REJOUER!!!
                    break;
                case ChoixMenu.QUITTER:
                    �tat = �tatsJeuT.MENU;
                    �tatMenu = �tatsMenu.MENU_PRINCIPAL;
                    MenuFinDePartie.Enabled = false;
                    MenuPrincipal.Enabled = true;
                    NetworkManager.SendDisconnect();
                    break;
            }
        }

        private void G�rerTransitionPerdu()
        {
            if (JoueurEstArriv�)
            {
                �tat = �tatsJeuT.FIN_DE_PARTIE;
                MenuFinDePartie.Enabled = true;
                NetworkManager.TempsDeCourseJ.EstActif = false;
                Joueur.EstActif = false;
                NetworkManager.SendTermin�(true, NetworkManager.TempsDeCourseJ.ValeurTimer, NetworkManager.PseudonymeJ);
            }
            else
            {
                Ennemi.AjusterPosition(NetworkManager.MatriceMondeEnnemi);
            }
        }

        private void G�rerTransitionGagn�()
        {
            if (NetworkManager.EnnemiEstArriv� || �tatJoueur == �tatsJoueur.SOLO)
            {
                �tat = �tatsJeuT.FIN_DE_PARTIE;
                MenuFinDePartie.Enabled = true;
                Joueur.Enabled = false;
            }
            else
            {
                Ennemi.AjusterPosition(NetworkManager.MatriceMondeEnnemi);
            }
        }

        private void G�rerTransitionStandBy()
        {
            if (NetworkManager.EnnemiPr�t�Jouer)
            {
                �tat = �tatsJeuT.JEU;
                Pause = false;
                MenuDePause.Enabled = false;
            }
        }

        private void G�rerTransitionPause()
        {
            switch (MenuDePause.Choix)
            {
                case ChoixMenu.JOUER:
                    MenuDePause.Enabled = false;
                    �tat = �tatsJeuT.JEU;
                    Pause = false; //AJOUTER D�COMPTE :(
                    break;
                case ChoixMenu.QUITTER:
                    MenuDePause.Enabled = false;
                    �tat = �tatsJeuT.MENU;
                    �tatMenu = �tatsMenu.MENU_PRINCIPAL;
                    MenuPrincipal.Enabled = true;
                    NetworkManager.SendDisconnect();
                    break;
                case ChoixMenu.OPTION:
                    �tatPr�c�dentOption = �tatMenu; //NOT WORKING... PAUSE != menu! yet
                    �tat = �tatsJeuT.MENU;
                    �tatMenu = �tatsMenu.MENU_OPTION;
                    MenuDesOptions.Enabled = true;
                    MenuDePause.ChangerActivationMenu(false);
                    MenuS�lectionn�Option = MenuDePause;
                    break;
            }
        }

        private void G�rerTransitionJeu()
        {
            G�rerChangementPause();
            G�rerGagnant();
        }

        private void G�rerGagnant()
        {
            if (NetworkManager.EnnemiEstArriv�)
            {
                Gagn� = false;
            }
            else
            {
                if (JoueurEstArriv�)
                {
                    if (�tatJoueur == �tatsJoueur.SOLO)
                    {
                        Gagn� = NetworkManager.TempsDeCourseJ.ValeurTimer < new TimeSpan(0, 0, 10); //obtenir vraie valeur
                    }
                    else
                    {
                        Gagn� = true;
                    }
                }
            }
        }


        private void G�rerChangementPause()
        {
            if (�tatJoueur != �tatsJoueur.CLIENT)
            {
                if (GestionInput.EstNouvelleTouche(Keys.Space))
                {
                    �tat = �tatsJeuT.PAUSE;
                    Pause = true;
                    MenuDePause.Enabled = true;
                    MenuDePause.ChangerActivationMenu(true);
                }
            }
            else
            {
                if (!NetworkManager.EnnemiPr�t�Jouer)
                {
                    �tat = �tatsJeuT.STAND_BY;
                    MenuDePause.Enabled = true;
                    MenuDePause.ChangerActivationMenu(false);
                    Pause = true;
                }
            }

        }
        private void G�rerTransitionD�compte()
        {
            if (!D�compteInitial.EstActif)
            {
                �tat = �tatsJeuT.JEU;
                Joueur.EstActif = true;
                D�compteInitial.Enabled = false;
                D�compteInitial.Visible = false;
                NetworkManager.TempsDeCourseJ = new TimerAugmente(Game, new TimeSpan(0), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, 30), "Blanc", true, 0.01f);
                Game.Components.Add(NetworkManager.TempsDeCourseJ);
            }
        }

        private void G�rerTransitionAttenteJoueurs()
        {
            switch (�tatJoueur)
            {
                case �tatsJoueur.SOLO:
                    MenuChoixProfile.ActiverBtnD�marrer();
                    �tatMenu = �tatsMenu.CHOIX_PROFILE;
                    break;
                case �tatsJoueur.CLIENT:
                    if (NetworkManager.EnnemiPr�t�Jouer)
                    {
                        MenuChoixProfile.Enabled = false;
                        InitialiserLeJeu();
                        �tat = �tatsJeuT.D�COMPTE;
                        InitialiserD�compte();
                    }
                    break;
                case �tatsJoueur.SERVEUR:
                    if (NetworkManager.EnnemiPr�t�Jouer)
                    {
                        �tatMenu = �tatsMenu.CHOIX_PROFILE;
                        MenuChoixProfile.ActiverBtnD�marrer();
                    }
                    break;
            }
        }





        //NOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOON! Trouver solution..?
        //bool t { get; set; }
        private void G�rerTransitionConnection()
        {
            if (/*Serveur.connectedClients == 2*/NetworkManager.enemiConnect�)
            {
                //t = true;
                //bool test = NetworkManager.enemiConnect�;
                �tatMenu = �tatsMenu.CHOIX_PROFILE;
                MenuServeur.Enabled = false;
                MenuChoixProfile.Enabled = true;
            }
        }

        private void G�rerTransitionMenuProfile()
        {
            //bool k = NetworkManager.enemiConnect�;
            //t = true;
            switch (MenuChoixProfile.Choix)
            {
                case ChoixMenu.EN_ATTENTE:
                    break;
                case ChoixMenu.VALIDATION:
                    if (�tatJoueur == �tatsJoueur.CLIENT)
                    {
                        NetworkManager.SendPr�tJeu(true);
                    }
                    �tatMenu = �tatsMenu.ATTENTE_JOUEURS;
                    NetworkManager.PseudonymeJ = MenuChoixProfile.Pseudonyme;
                    //ENVOYER PSUDONYME A ENNEMI!!!!!!!
                    break;
                case ChoixMenu.JOUER:
                    //retirer tous les menus des components?
                    �tat = �tatsJeuT.D�COMPTE;
                    MenuChoixProfile.Enabled = false;
                    InitialiserLeJeu();
                    InitialiserD�compte();
                    break;
            }
        }

        private void InitialiserD�compte()
        {
            D�compteInitial = new TimerDiminue(Game, new TimeSpan(0, 0, 5), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2), "Blanc", true, 1f);
            Game.Components.Add(D�compteInitial);
        }

        void G�rerTransitionMenuPrincipal()
        {
            switch (MenuPrincipal.Choix)
            {
                case ChoixMenu.EN_ATTENTE:
                    break;
                case ChoixMenu.JOUER:
                    �tatMenu = �tatsMenu.CHOIX_LAN;
                    MenuPrincipal.Enabled = false;
                    MenuNetwork.Enabled = true;
                    break;
                case ChoixMenu.OPTION:
                    �tatPr�c�dentOption = �tatMenu;
                    �tatMenu = �tatsMenu.MENU_OPTION;
                    MenuPrincipal.ChangerActivationMenu(false);
                    MenuDesOptions.Enabled = true;
                    MenuS�lectionn�Option = MenuPrincipal;
                    break;
                case ChoixMenu.QUITTER:
                    Game.Exit();
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
                    �tatMenu = �tatPr�c�dentOption;
                    MenuDesOptions.Enabled = false;
                    MenuS�lectionn�Option.ChangerActivationMenu(true);
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
                    �tatMenu = �tatsMenu.CHOIX_PROFILE;
                    MenuNetwork.Enabled = false;
                    MenuChoixProfile.Enabled = true;
                    �tatJoueur = �tatsJoueur.SOLO;
                    ConnectionAuServeur("127.0.0.1", 5001);
                    break;
                case ChoixMenu.REJOINDRE:
                    �tatMenu = �tatsMenu.ENTR�E_PORT_CLIENT;
                    MenuNetwork.Enabled = false;
                    MenuClient.Enabled = true;
                    �tatJoueur = �tatsJoueur.CLIENT;
                    break;
                case ChoixMenu.SERVEUR:
                    �tatMenu = �tatsMenu.ENTR�E_PORT_SERVEUR;
                    MenuNetwork.Enabled = false;
                    MenuServeur.Enabled = true;
                    �tatJoueur = �tatsJoueur.SERVEUR;
                    break;
                case ChoixMenu.RETOUR:
                    �tatMenu = �tatsMenu.MENU_PRINCIPAL;
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
                    �tatMenu = �tatsMenu.CONNECTION;
                    ConnectionAuServeur(MenuServeur.IP, MenuServeur.Port);
                    break;
                case ChoixMenu.QUITTER:
                    �tatMenu = �tatsMenu.CHOIX_LAN;
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
                    �tatMenu = �tatsMenu.CHOIX_PROFILE;
                    MenuClient.Enabled = false;
                    MenuChoixProfile.Enabled = true;
                    ConnectionAuServeur(MenuClient.IP, MenuClient.Port);
                    break;
                case ChoixMenu.QUITTER:
                    �tatMenu = �tatsMenu.CHOIX_LAN;
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
                NetworkManager = new R�seautique(Game,/*Serveur,*/ ip, port);
                Game.Services.AddService(typeof(R�seautique), NetworkManager);
                UsedIP.Add(ip);
            }
        }
        #region initialisation du jeu
        private void InitialiserLeJeu()
        {
            Reset();
            Cr�erVoitureDummy();
            Cr�erJoueur();
            if (�tatJoueur != �tatsJoueur.SOLO)
            {
                Cr�erEnnemi();
            }
        }

        private void Cr�erVoitureDummy()
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

        private void Cr�erEnvironnement()
        {
            Game.Components.Add(new Arri�rePlanD�roulant(Game, "Ciel�toil�", 0.01f));
            Sections = new List<Section>();

            �tendueTotale = new Vector2(200 * 4, 200 * 4); //envoyer � voiture?
            for (int i = 0; i < 10; ++i)
            {
                for (int j = 0; j < 10; ++j)
                {
                    Section newSection = new Section(Game, new Vector2(�TENDUE * i, �TENDUE * j), new Vector2(�TENDUE, �TENDUE), 1f, Vector3.Zero, Vector3.Zero, new Vector3(�TENDUE, 25, �TENDUE), new string[] { "Herbe", "Sable" }, 0.01f); //double??
                    Sections.Add(newSection);
                    Game.Components.Add(newSection);
                    newSection.DrawOrder = 0; //le terrain doit �tre dessin� en 2e
                }
            }
            Game.Components.Add(new Maison(Game, 10f, Vector3.Zero, new Vector3(500, 0, 400), new Vector3(2, 2, 2), "Carte", "BoutonVert", 0.01f));

        }

        private void Cr�erEnnemi()
        {
            //Vrai Posinitiale :(
            Ennemi = new Voiture(Game, "GLX_400", 0.01f, Vector3.Zero, NetworkManager.PositionEnnemi, 0.01f); //Get choix de voiture??
            Ennemi.EstActif = false;
            Game.Components.Add(Ennemi);
        }

        private void Cr�erJoueur()
        {
            Joueur = new Voiture(Game, "GLX_400", 0.01f, Vector3.Zero, new Vector3(100, 0, 50), 0.01f); //mettre choix?
            Joueur.EstActif = false;
            Game.Components.Add(Joueur);
            NetworkManager.SendPosIni(Joueur.Position); //fonctionne pas....
        }

        private void Cr�erCam�ra()
        {
            Vector3 positionCam�ra = new Vector3(100, 50, 125);
            Vector3 cibleCam�ra = new Vector3(100, 0, 50);
            Cam�raSubjective Cam�raJeu = new Cam�raSubjective(Game, positionCam�ra, cibleCam�ra, Vector3.Up, 0.01f);
            Game.Components.Add(Cam�raJeu);
            Game.Services.AddService(typeof(Cam�ra), Cam�raJeu);
        }
        #endregion

        #region Cr�ation Des Menus
        void Cr�erMenus()
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
