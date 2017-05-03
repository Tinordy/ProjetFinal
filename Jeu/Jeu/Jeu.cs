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
//using System.Windows.Forms;
using System.IO;
using System.Net;

namespace AtelierXNA
{
    //GROSSE CLASSE MENUSDEJEU???
    //
    enum �tatsJeu { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, JEU, CHOIX_PROFILE, ENTR�E_PORT_SERVEUR, ENTR�E_PORT_CLIENT, MENU_PAUSE, CONNECTION, ATTENTE_JOUEURS, D�COMPTE, PAUSE, STAND_BY, GAGN�, PERDU, FIN_DE_PARTIE, MENU }
    enum �tatsJoueur { SOLO, SERVEUR, CLIENT }
    enum �tatsMenu { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, CHOIX_PROFILE, ENTR�E_PORT_SERVEUR, ENTR�E_PORT_CLIENT, ATTENTE_JOUEURS, CONNECTION }
    public class Jeu : Microsoft.Xna.Framework.GameComponent
    {
        const int NB_VOITURES_DUMMIES = 10;
        const int LARGEUR_ENTRE_VOITURE = 100;
        const int NB_TOURS = 3;
        const int LARGEUR_D�PART = 3;
        Vector3 RotationInitiale = new Vector3(0, 3.14f, 0);
        bool Connection�tablie { get; set; }
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
                foreach(VoitureDummy v in VoituresDummies)
                {
                    v.Enabled = !pause;
                }
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
                    �tat = �tatsJeu.GAGN�;
                    NetworkManager.TempsDeCourseJ.EstActif = false;
                }
                else
                {
                    �tat = �tatsJeu.PERDU;
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
                return Banni�re.Est�Arriv�e(Joueur.Position);
            }
        }
        �tatsJeu �tat { get; set; }
        �tatsJoueur �tatJoueur { get; set; }
        MenuPrincipal MenuPrincipal { get; set; }
        MenuOption MenuDesOptions { get; set; }
        MenuLan MenuNetwork { get; set; }
        MenuProfile MenuChoixProfile { get; set; }
        MenuIPServeur MenuServeur { get; set; }
        MenuIPClient MenuClient { get; set; }
        MenuPause MenuDePause { get; set; }
        MenuFinPartie MenuFinDePartie { get; set; }
        Menu MenuS�lectionn�Option { get; set; }
        �tatsJeu �tatPr�c�dentOption { get; set; }

        Server Serveur { get; set; }
        R�seautique NetworkManager { get; set; }
        Voiture Joueur { get; set; }
        Voiture Ennemi { get; set; }
        List<Section> Sections { get; set; }
        TimerDiminue D�compteInitial { get; set; }
        //TimerAugmente TempsDeCourse { get; set; }
        Vector2 �tendueTotale { get; set; }
        InputManager GestionInput { get; set; }
        Banni�reArriv�e Banni�re { get; set; }
        BoundingSphere[] CheckPoints { get; set; }
        int NbTours { get; set; }
        bool[] CheckPointsAtteints { get; set; }
        List<Voiture> VoituresDummies { get; set; }
        List<string> ChoixVoiture = new List<string>() { "GLX_400", "FBX_2010_chevy_volt_deepRed", "volks", "FBX_2008_dodge_charger_blue" };//meilleures voitures plz
        Titre AffNbTours { get; set; }

        public Jeu(Game game)
            : base(game)
        {
            Cr�erMenus();
            test = false;
        }
        public override void Initialize()
        {
            CheckPointsAtteints = new bool[2] { false, false };
            NbTours = 0;
            �tatJoueur = �tatsJoueur.SOLO;
            Cr�erCam�ra();
            Cr�erEnvironnement();
            Game.Components.Add(new AfficheurFPS(Game, "Arial20", Color.White, 1f));

            UsedIP = new List<string>();
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            �tat = �tatsJeu.MENU_PRINCIPAL;
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
            if (Connection�tablie && NetworkManager.DisconnectedT)
            {
                �tat = �tatsJeu.MENU_PRINCIPAL;
                MenuPrincipal.Enabled = true;
                //d�sactiver tous les menus
                MenuDePause.Enabled = false;
                MenuFinDePartie.Enabled = false;
                NetworkManager.DisconnectedT = false;
                Connection�tablie = false;
                NetworkManager.Close();

            }
        }

        private void G�rer�tat()
        {
            if(�tat == �tatsJeu.JEU||�tat == �tatsJeu.GAGN�||�tat == �tatsJeu.PERDU)
            {
                G�rer�tatJeu();
            }
            //switch (�tat)
            //{
            //    case �tatsJeu.JEU:
            //        G�rer�tatJeu();
            //        break;
            //    case �tatsJeu.PERDU:
            //        break;
            //    case �tatsJeu.GAGN�:
            //        break;
            //}
        }

        private void G�rer�tatJeu()
        {
            //if player moved?
            if (�tatJoueur != �tatsJoueur.SOLO /*&& NetworkManager.EnnemiPr�t�Jouer*/) //TEST
            {
                Ennemi.AjusterPosition(NetworkManager.MatriceMondeEnnemi);
            }
            G�rerCollisions();
            G�rerCheckPoints();
            //Collision, nb tours?
        }

        private void G�rerCheckPoints()
        {
            if (CheckPoints[0].Contains(Joueur.Position) == ContainmentType.Contains)
            {
                CheckPointsAtteints[0] = true;
            }
            if(CheckPointsAtteints[0] && CheckPoints[1].Contains(Joueur.Position) == ContainmentType.Contains)
            {
                CheckPointsAtteints[1] = true;
            }
            if (CheckPointsAtteints[1] && JoueurEstArriv�)
            {
                ++NbTours;
                CheckPointsAtteints = new bool[2] { false, false };
                AffNbTours.ChangerTexte("Tour : " + NbTours);
            }
        }

        private void G�rerCollisions()
        {
            //collision entre joueurs
            if (�tatJoueur != �tatsJoueur.SOLO && Joueur.EstEnCollision2(Ennemi))
            {
                Joueur.Rebondir(Ennemi.Direction, Ennemi.Position); //SWITCH
            }
            //collision avec objets
            bool estEnColAvecObj = false;
            int i = 0;
            Vector3 centre = Vector3.Zero;
            while (!estEnColAvecObj && i < Sections.Count)
            {
                estEnColAvecObj = Sections[i].EstEnCollisionAvecUnObjet(Joueur);
                if (Sections[i].EstEnCollisionAvecUnObjet(Joueur))
                {
                    centre = Sections[i].SphereDeCollision.Center;
                }
                ++i;
            }
            i = 0;
            while (!estEnColAvecObj && i < VoituresDummies.Count)
            {
                estEnColAvecObj = Joueur.EstEnCollision(VoituresDummies[i]);
                ++i;
            }
            if (estEnColAvecObj)
            {
                Joueur.Rebondir(Vector3.Zero, centre);              
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
                case �tatsJeu.D�COMPTE:
                    G�rerTransitionD�compte();
                    break;
                case �tatsJeu.JEU:
                    G�rerTransitionJeu();
                    break;
                case �tatsJeu.PAUSE:
                    G�rerTransitionPause();
                    break;
                case �tatsJeu.STAND_BY:
                    G�rerTransitionStandBy();
                    break;
                case �tatsJeu.GAGN�:
                    G�rerTransitionGagn�();
                    break;
                case �tatsJeu.PERDU:
                    G�rerTransitionPerdu();
                    break;
                case �tatsJeu.FIN_DE_PARTIE:
                    G�rerTransitionFinDePartie();
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
                    �tat = �tatsJeu.CHOIX_PROFILE;
                    //Envoyer client!!! REJOUER!!!
                    break;
                case ChoixMenu.QUITTER:
                    �tat = �tatsJeu.MENU_PRINCIPAL;
                    MenuFinDePartie.Enabled = false;
                    MenuPrincipal.Enabled = true;
                    NetworkManager.SendDisconnect();
                    Connection�tablie = false;
                    NetworkManager.Close();
                    break;
            }
        }

        private void G�rerTransitionPerdu()
        {
            if (NbTours == NB_TOURS)
            {
                �tat = �tatsJeu.FIN_DE_PARTIE;
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
                �tat = �tatsJeu.FIN_DE_PARTIE;
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
                �tat = �tatsJeu.JEU;
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
                    �tat = �tatsJeu.JEU;
                    Pause = false; //AJOUTER D�COMPTE :(
                    break;
                case ChoixMenu.QUITTER:
                    MenuDePause.Enabled = false;
                    �tat = �tatsJeu.MENU_PRINCIPAL;
                    MenuPrincipal.Enabled = true;

                    NetworkManager.SendDisconnect();
                    NetworkManager.Close();
                    //s'arranger pour que ce soit automatique?
                    Connection�tablie = false;
                    //tests!

                    break;
                case ChoixMenu.OPTION:
                    �tatPr�c�dentOption = �tat;
                    �tat = �tatsJeu.MENU_OPTION;
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
                if (NbTours == NB_TOURS)
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
                    �tat = �tatsJeu.PAUSE;
                    Pause = true;
                    MenuDePause.Enabled = true;
                    MenuDePause.ChangerActivationMenu(true);
                }
            }
            else
            {
                if (!NetworkManager.EnnemiPr�t�Jouer)
                {
                    �tat = �tatsJeu.STAND_BY;
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
                �tat = �tatsJeu.JEU;
                Joueur.EstActif = true;
                D�compteInitial.Enabled = false;
                D�compteInitial.Visible = false;
                NetworkManager.TempsDeCourseJ = new TimerAugmente(Game, new TimeSpan(0), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, 30), "Blanc", true, false, Color.White, INTERVALLE_MAJ);
                Game.Components.Add(NetworkManager.TempsDeCourseJ);
                MenuDesOptions.D�sactiverDifficult�();
                AffNbTours = new Titre(Game, "Tour : 0", "Arial20", new Vector2(Game.Window.ClientBounds.Width / 15, Game.Window.ClientBounds.Height / 18), "Blanc", false, Color.White);
                Game.Components.Add(AffNbTours);
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
                        MenuChoixProfile.Enabled = false;
                        InitialiserLeJeu();
                        �tat = �tatsJeu.D�COMPTE;
                        InitialiserD�compte();
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





        //NOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOON! Trouver solution..?
        //bool t { get; set; }
        private void G�rerTransitionConnection()
        {
            if (/*Serveur.connectedClients == 2*/NetworkManager.enemiConnect�)
            {
                //t = true;
                //bool test = NetworkManager.enemiConnect�;
                �tat = �tatsJeu.CHOIX_PROFILE;
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
                    �tat = �tatsJeu.ATTENTE_JOUEURS;
                    ChercherPseudonyme();
                    break;
                case ChoixMenu.JOUER:
                    �tat = �tatsJeu.D�COMPTE;
                    MenuChoixProfile.Enabled = false;
                    InitialiserLeJeu();
                    InitialiserD�compte();
                    break;
            }
        }

        private void ChercherPseudonyme()
        {
            if (MenuChoixProfile.Pseudonyme == string.Empty)
            {
                if (�tatJoueur != �tatsJoueur.CLIENT)
                {
                    MenuChoixProfile.Pseudonyme = "JOUEUR 1";
                }
                else
                {
                    MenuChoixProfile.Pseudonyme = "JOUEUR 2";
                }
            }
            NetworkManager.PseudonymeJ = MenuChoixProfile.Pseudonyme;
        }

        private void InitialiserD�compte()
        {
            D�compteInitial = new TimerDiminue(Game, new TimeSpan(0, 0, 5), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2), "Blanc", true,false, Color.White, 1f);
            Game.Components.Add(D�compteInitial);
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
                    �tatPr�c�dentOption = �tat;
                    �tat = �tatsJeu.MENU_OPTION;
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
                    �tat = �tatPr�c�dentOption;
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
                    �tat = �tatsJeu.CHOIX_PROFILE;
                    MenuNetwork.Enabled = false;
                    MenuChoixProfile.Enabled = true;
                    �tatJoueur = �tatsJoueur.SOLO;
                    //test
                    //string HostName = Dns.GetHostName();
                    //ConnectionAuServeur(Dns.GetHostAddresses(HostName)[1].ToString(), 5001);
                    ConnectionAuServeur("127.0.0.1", 5001);
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
        bool test { get; set; }

        private void ConnectionAuServeur(string ip, int port)
        {
            //enlever ip!!!
            if (!test/*UsedIP.FindIndex(s => s == ip) == -1*/) //marche simple joueur, pt pas multijoueur?
            {
                if (�tatJoueur != �tatsJoueur.CLIENT)
                {
                    Serveur = new Server(port, ip);
                    Game.Services.AddService(typeof(Server), Serveur);
                }
                NetworkManager = new R�seautique(Game,/*Serveur,*/ ip, port);
                NetworkManager.SetEnnemi(MenuDesOptions.GetDifficult�());
                Game.Services.AddService(typeof(R�seautique), NetworkManager);
                //UsedIP.Add(ip);
                test = true;
            }
            else
            {
                NetworkManager.Reset(ip, port);
            }
            Connection�tablie = true;
            //NetworkManager = new R�seautique(/*Serveur,*/ ip, port);
            //Game.Services.AddService(typeof(R�seautique), NetworkManager);

        }
        #region initialisation du jeu
        private void InitialiserLeJeu()
        {
            Reset();
            Cr�erVoitureDummy();
            Vector2 v = Sections[10].ObtenirPointD�part();
            if (�tatJoueur != �tatsJoueur.SOLO)
            {
                Cr�erEnnemi(v);
            }
            Cr�erJoueur(v);  
        }

        private void Cr�erVoitureDummy()
        {
            VoituresDummies = new List<Voiture>();
            Voiture temp;
            for (int i = 0; i < NB_VOITURES_DUMMIES; ++i)
            {
                temp = new VoitureDummy(Game, "small car", 0.01f, Vector3.Zero, new Vector3(50, 5, 50), i * LARGEUR_ENTRE_VOITURE, INTERVALLE_MAJ);
                VoituresDummies.Add(temp);
                Game.Components.Add(temp);
            }
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
            CheckPointsAtteints = new bool[2] { false, false };
            NbTours = 0;
            Game.Components.Remove(AffNbTours);
        }

        private void Cr�erEnvironnement()
        {
            Game.Components.Add(new Arri�rePlanD�roulant(Game, "Ciel�toil�", INTERVALLE_MAJ));
            Sections = new List<Section>();

            �tendueTotale = new Vector2(200 * 4, 200 * 4); //envoyer � voiture?
            List<int> pasDeMaison = new List<int>() { 17, 25, 32, 33, 24, 47, 61, 53, 60, 63, 64, 51, 49, 50, 43, 36, 37, 29, 30, 16, 37, 21, 22, 23 };
            for (int i = 0; i < 10; ++i)
            {
                for (int j = 0; j < 7; ++j)
                {
                    bool maison = !pasDeMaison.Contains(Sections.Count);
                    Section newSection = new Section(Game, new Vector2(�TENDUE * i, �TENDUE * j), new Vector2(�TENDUE, �TENDUE), 1f, Vector3.Zero, Vector3.Zero, new Vector3(�TENDUE, 25, �TENDUE), new string[] { "Neige", "Sable" }, maison, INTERVALLE_MAJ); //double??
                    Sections.Add(newSection);
                    newSection.Initialize();
                    Game.Components.Add(newSection);
                }
            }

            Banni�re = Sections[10].Cr�erBanni�re();
            CheckPoints = new BoundingSphere[2];
            CheckPoints[0] = Sections[54].Cr�erCheckPoint();
            CheckPoints[1] = Sections[63].Cr�erCheckPoint();
            Sections[10].Initialize();
            Sections[10].Visible = true;
        }

        private void Cr�erEnnemi(Vector2 D�part)
        {
            Vector3 pos = �tatJoueur == �tatsJoueur.SERVEUR ? new Vector3(D�part.X - LARGEUR_D�PART, 0, D�part.Y) : new Vector3(D�part.X + LARGEUR_D�PART, 0, D�part.Y);
            Ennemi = new Voiture(Game, ChoixVoiture[NetworkManager.ChoixVoitureE], 0.01f, Vector3.Zero, pos, INTERVALLE_MAJ); //Get choix de voiture??
            Ennemi.EstActif = false;
            Game.Components.Add(Ennemi);
        }

        private void Cr�erJoueur(Vector2 D�part)
        {
            Vector3 pos = �tatJoueur != �tatsJoueur.CLIENT ? new Vector3(LARGEUR_D�PART + D�part.X, 0, D�part.Y) : new Vector3(D�part.X - LARGEUR_D�PART, 0, D�part.Y);
            Joueur = new Voiture(Game, ChoixVoiture[NetworkManager.ChoixVoitureJ], 0.01f, RotationInitiale, pos, INTERVALLE_MAJ);
            Joueur.EstActif = false;
            Game.Components.Add(Joueur);
            NetworkManager.SendPosIni(Joueur.Position); //fonctionne pas....
        }

        private void Cr�erCam�ra()
        {
            Vector3 positionCam�ra = new Vector3(100, 50, 125);
            Vector3 cibleCam�ra = new Vector3(100, 0, 50);
            Cam�raSubjective Cam�raJeu = new Cam�raSubjective(Game, positionCam�ra, cibleCam�ra, Vector3.Up, INTERVALLE_MAJ);
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
