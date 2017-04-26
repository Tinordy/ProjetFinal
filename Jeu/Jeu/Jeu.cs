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
    enum ÉtatsJeu { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, JEU, CHOIX_PROFILE, ENTRÉE_PORT_SERVEUR, ENTRÉE_PORT_CLIENT, MENU_PAUSE, CONNECTION, ATTENTE_JOUEURS, DÉCOMPTE, PAUSE, STAND_BY, GAGNÉ, PERDU, FIN_DE_PARTIE, MENU }
    enum ÉtatsJoueur { SOLO, SERVEUR, CLIENT }
    enum ÉtatsMenu { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, CHOIX_PROFILE, ENTRÉE_PORT_SERVEUR, ENTRÉE_PORT_CLIENT, ATTENTE_JOUEURS, CONNECTION }
    public class Jeu : Microsoft.Xna.Framework.GameComponent
    {
        const int NB_VOITURES_DUMMIES = 10;
        const int LARGEUR_ENTRE_VOITURE = 100;
        const int NB_TOURS = 1;
        const int LARGEUR_DÉPART = 3;
        Vector3 RotationInitiale = new Vector3(0, 3.14f, 0);
        bool ConnectionÉtablie { get; set; }
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
                    État = ÉtatsJeu.GAGNÉ;
                    NetworkManager.TempsDeCourseJ.EstActif = false;
                }
                else
                {
                    État = ÉtatsJeu.PERDU;
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
                return Bannière.EstÀArrivée(Joueur.Position);
            }
        }
        ÉtatsJeu État { get; set; }
        ÉtatsJoueur ÉtatJoueur { get; set; }
        MenuPrincipal MenuPrincipal { get; set; }
        MenuOption MenuDesOptions { get; set; }
        MenuLan MenuNetwork { get; set; }
        MenuProfile MenuChoixProfile { get; set; }
        MenuIPServeur MenuServeur { get; set; }
        MenuIPClient MenuClient { get; set; }
        MenuPause MenuDePause { get; set; }
        MenuFinPartie MenuFinDePartie { get; set; }
        Menu MenuSélectionnéOption { get; set; }
        ÉtatsJeu ÉtatPrécédentOption { get; set; }

        Server Serveur { get; set; }
        Réseautique NetworkManager { get; set; }
        Voiture Joueur { get; set; }
        Voiture Ennemi { get; set; }
        List<Section> Sections { get; set; }
        TimerDiminue DécompteInitial { get; set; }
        //TimerAugmente TempsDeCourse { get; set; }
        Vector2 ÉtendueTotale { get; set; }
        InputManager GestionInput { get; set; }
        BannièreArrivée Bannière { get; set; }
        BoundingSphere CheckPoint { get; set; }
        int NbTours { get; set; }
        bool CheckPointAtteint { get; set; }
        List<Voiture> VoituresDummies { get; set; }

        public Jeu(Game game)
            : base(game)
        {
            CréerMenus();
            test = false;
        }
        public override void Initialize()
        {
            CheckPointAtteint = false;
            NbTours = 0;
            ÉtatJoueur = ÉtatsJoueur.SOLO;
            CréerCaméra();
            CréerEnvironnement();
            Game.Components.Add(new AfficheurFPS(Game, "Arial20", Color.White, 1f));

            UsedIP = new List<string>();
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            État = ÉtatsJeu.MENU_PRINCIPAL;
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
            if (ConnectionÉtablie && NetworkManager.DisconnectedT)
            {
                État = ÉtatsJeu.MENU_PRINCIPAL;
                MenuPrincipal.Enabled = true;
                //désactiver tous les menus
                MenuDePause.Enabled = false;
                MenuFinDePartie.Enabled = false;
                NetworkManager.DisconnectedT = false;
                ConnectionÉtablie = false;
                NetworkManager.Close();

            }
        }

        private void GérerÉtat()
        {
            switch (État)
            {
                case ÉtatsJeu.JEU:
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
            }
            GérerCollisions();
            GérerCheckPoints();
            //Collision, nb tours?
        }

        private void GérerCheckPoints()
        {
            if (CheckPoint.Contains(Joueur.Position) == ContainmentType.Contains)
            {
                CheckPointAtteint = true;
            }
            if (CheckPointAtteint && JoueurEstArrivé)
            {
                ++NbTours;
                CheckPointAtteint = false;
            }
        }

        private void GérerCollisions()
        {
            //collision entre joueurs
            if (ÉtatJoueur != ÉtatsJoueur.SOLO && Joueur.EstEnCollision(Ennemi))
            {
                Joueur.Rebondir(Vector2.Zero); //SWITCH
            }
            //collision avec objets
            bool estEnColAvecObj = false;
            int i = 0;
            while (!estEnColAvecObj && i < Sections.Count)
            {
                estEnColAvecObj = Sections[i].EstEnCollisionAvecUnObjet(Joueur);
                ++i;
            }
            while(!estEnColAvecObj && i < VoituresDummies.Count)
            {
                estEnColAvecObj = Joueur.EstEnCollision(VoituresDummies[i]);
            }
            if (estEnColAvecObj)
            {
                Joueur.Rebondir(Vector2.Zero);
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
                case ÉtatsJeu.DÉCOMPTE:
                    GérerTransitionDécompte();
                    break;
                case ÉtatsJeu.JEU:
                    GérerTransitionJeu();
                    break;
                case ÉtatsJeu.PAUSE:
                    GérerTransitionPause();
                    break;
                case ÉtatsJeu.STAND_BY:
                    GérerTransitionStandBy();
                    break;
                case ÉtatsJeu.GAGNÉ:
                    GérerTransitionGagné();
                    break;
                case ÉtatsJeu.PERDU:
                    GérerTransitionPerdu();
                    break;
                case ÉtatsJeu.FIN_DE_PARTIE:
                    GérerTransitionFinDePartie();
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
                    État = ÉtatsJeu.CHOIX_PROFILE;
                    //Envoyer client!!! REJOUER!!!
                    break;
                case ChoixMenu.QUITTER:
                    État = ÉtatsJeu.MENU_PRINCIPAL;
                    MenuFinDePartie.Enabled = false;
                    MenuPrincipal.Enabled = true;
                    NetworkManager.SendDisconnect();
                    ConnectionÉtablie = false;
                    NetworkManager.Close();
                    break;
            }
        }

        private void GérerTransitionPerdu()
        {
            if (JoueurEstArrivé)
            {
                État = ÉtatsJeu.FIN_DE_PARTIE;
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
                État = ÉtatsJeu.FIN_DE_PARTIE;
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
                État = ÉtatsJeu.JEU;
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
                    État = ÉtatsJeu.JEU;
                    Pause = false; //AJOUTER DÉCOMPTE :(
                    break;
                case ChoixMenu.QUITTER:
                    MenuDePause.Enabled = false;
                    État = ÉtatsJeu.MENU_PRINCIPAL;
                    MenuPrincipal.Enabled = true;

                    NetworkManager.SendDisconnect();
                    NetworkManager.Close();
                    //s'arranger pour que ce soit automatique?
                    ConnectionÉtablie = false;
                    //tests!

                    break;
                case ChoixMenu.OPTION:
                    ÉtatPrécédentOption = État;
                    État = ÉtatsJeu.MENU_OPTION;
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
                if (NbTours == NB_TOURS)
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
                    État = ÉtatsJeu.PAUSE;
                    Pause = true;
                    MenuDePause.Enabled = true;
                    MenuDePause.ChangerActivationMenu(true);
                }
            }
            else
            {
                if (!NetworkManager.EnnemiPrêtÀJouer)
                {
                    État = ÉtatsJeu.STAND_BY;
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
                État = ÉtatsJeu.JEU;
                Joueur.EstActif = true;
                DécompteInitial.Enabled = false;
                DécompteInitial.Visible = false;
                NetworkManager.TempsDeCourseJ = new TimerAugmente(Game, new TimeSpan(0), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, 30), "Blanc", true, INTERVALLE_MAJ);
                Game.Components.Add(NetworkManager.TempsDeCourseJ);
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
                        MenuChoixProfile.Enabled = false;
                        InitialiserLeJeu();
                        État = ÉtatsJeu.DÉCOMPTE;
                        InitialiserDécompte();
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





        //NOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOON! Trouver solution..?
        //bool t { get; set; }
        private void GérerTransitionConnection()
        {
            if (/*Serveur.connectedClients == 2*/NetworkManager.enemiConnecté)
            {
                //t = true;
                //bool test = NetworkManager.enemiConnecté;
                État = ÉtatsJeu.CHOIX_PROFILE;
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
                    État = ÉtatsJeu.ATTENTE_JOUEURS;
                    ChercherPseudonyme();
                    break;
                case ChoixMenu.JOUER:
                    État = ÉtatsJeu.DÉCOMPTE;
                    MenuChoixProfile.Enabled = false;
                    InitialiserLeJeu();
                    InitialiserDécompte();
                    break;
            }
        }

        private void ChercherPseudonyme()
        {
            if (MenuChoixProfile.Pseudonyme == string.Empty)
            {
                if (ÉtatJoueur != ÉtatsJoueur.CLIENT)
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
                    État = ÉtatsJeu.CHOIX_LAN;
                    MenuPrincipal.Enabled = false;
                    MenuNetwork.Enabled = true;
                    break;
                case ChoixMenu.OPTION:
                    ÉtatPrécédentOption = État;
                    État = ÉtatsJeu.MENU_OPTION;
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
                    État = ÉtatPrécédentOption;
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
                    État = ÉtatsJeu.CHOIX_PROFILE;
                    MenuNetwork.Enabled = false;
                    MenuChoixProfile.Enabled = true;
                    ÉtatJoueur = ÉtatsJoueur.SOLO;
                    //test
                    //string HostName = Dns.GetHostName();
                    //ConnectionAuServeur(Dns.GetHostAddresses(HostName)[1].ToString(), 5001);
                    ConnectionAuServeur("127.0.0.1", 5001);
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
        bool test { get; set; }

        private void ConnectionAuServeur(string ip, int port)
        {
            //enlever ip!!!
            if (!test/*UsedIP.FindIndex(s => s == ip) == -1*/) //marche simple joueur, pt pas multijoueur?
            {
                if (ÉtatJoueur != ÉtatsJoueur.CLIENT)
                {
                    Serveur = new Server(port, ip);
                    Game.Services.AddService(typeof(Server), Serveur);
                }
                NetworkManager = new Réseautique(Game,/*Serveur,*/ ip, port);
                Game.Services.AddService(typeof(Réseautique), NetworkManager);
                //UsedIP.Add(ip);
                test = true;
            }
            else
            {
                NetworkManager.Reset(ip, port);
            }
            ConnectionÉtablie = true;
            //NetworkManager = new Réseautique(/*Serveur,*/ ip, port);
            //Game.Services.AddService(typeof(Réseautique), NetworkManager);

        }
        #region initialisation du jeu
        private void InitialiserLeJeu()
        {
            Reset();
            CréerVoitureDummy();
            Vector2 v = Sections[10].ObtenirPointDépart();
            CréerJoueur(v);
            if (ÉtatJoueur != ÉtatsJoueur.SOLO)
            {
                CréerEnnemi(v);
            }
        }

        private void CréerVoitureDummy()
        {
            VoituresDummies = new List<Voiture>();
            Voiture temp;
            for(int i = 0; i < NB_VOITURES_DUMMIES; ++i)
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
            CheckPointAtteint = false;
            NbTours = 0;
        }

        private void CréerEnvironnement()
        {
            Game.Components.Add(new ArrièrePlanDéroulant(Game, "CielÉtoilé", INTERVALLE_MAJ));
            Sections = new List<Section>();

            ÉtendueTotale = new Vector2(200 * 4, 200 * 4); //envoyer à voiture?
            List<int> pasDeMaison = new List<int>() { /*16, 23, 25, 28, 29, 35, 36, 54, 61 , 17,30,32,37,39,56,57*/};
            for (int i = 0; i < 10; ++i)
            {
                for (int j = 0; j < 7; ++j)
                {
                    bool maison = !pasDeMaison.Contains(Sections.Count);
                    Section newSection = new Section(Game, new Vector2(ÉTENDUE * i, ÉTENDUE * j), new Vector2(ÉTENDUE, ÉTENDUE), 1f, Vector3.Zero, Vector3.Zero, new Vector3(ÉTENDUE, 25, ÉTENDUE), new string[] { "HerbeSections", "Sable" }, maison,INTERVALLE_MAJ); //double??
                    Sections.Add(newSection);
                    newSection.Initialize();
                    Game.Components.Add(newSection);
                }
            }

            Bannière = Sections[10].CréerBannière();
            CheckPoint = Sections[63].CréerCheckPoint();
        }

        private void CréerEnnemi(Vector2 Départ)
        {
            if(ÉtatJoueur == ÉtatsJoueur.SERVEUR)
            {
                Ennemi = new Voiture(Game, "GLX_400", 0.01f, Vector3.Zero, new Vector3(Départ.X - LARGEUR_DÉPART, 0, Départ.Y), INTERVALLE_MAJ); //Get choix de voiture??
            }
            else
            {
                Ennemi = new Voiture(Game, "GLX_400", 0.01f, Vector3.Zero, new Vector3(Départ.X + LARGEUR_DÉPART, 0, Départ.Y), INTERVALLE_MAJ); //Get choix de voiture??
            }
            Ennemi.EstActif = false;
            Game.Components.Add(Ennemi);
        }

        private void CréerJoueur(Vector2 Départ)
        {
            if (ÉtatJoueur != ÉtatsJoueur.CLIENT)
            {
                Joueur = new Voiture(Game, "GLX_400", 0.01f, RotationInitiale, new Vector3(LARGEUR_DÉPART + Départ.X, 0, Départ.Y), INTERVALLE_MAJ); //mettre choix?
            }
            else
            {
                Joueur = new Voiture(Game, "GLX_400", 0.01f, RotationInitiale, new Vector3(Départ.X - LARGEUR_DÉPART, 0, Départ.Y), INTERVALLE_MAJ); //mettre choix?
            }
            Joueur.EstActif = false;
            Game.Components.Add(Joueur);
            NetworkManager.SendPosIni(Joueur.Position); //fonctionne pas....
        }

        private void CréerCaméra()
        {
            Vector3 positionCaméra = new Vector3(100, 50, 125);
            Vector3 cibleCaméra = new Vector3(100, 0, 50);
            CaméraSubjective CaméraJeu = new CaméraSubjective(Game, positionCaméra, cibleCaméra, Vector3.Up, INTERVALLE_MAJ);
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
