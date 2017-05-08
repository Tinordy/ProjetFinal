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
    enum ÉtatsJeu { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, JEU, CHOIX_PROFILE, ENTRÉE_PORT_SERVEUR, ENTRÉE_PORT_CLIENT, MENU_PAUSE, CONNECTION, ATTENTE_JOUEURS, DÉCOMPTE, PAUSE, STAND_BY, GAGNÉ, PERDU, FIN_DE_PARTIE, MENU }
    enum ÉtatsJoueur { SOLO, SERVEUR, CLIENT }
    enum ÉtatsMenu { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, CHOIX_PROFILE, ENTRÉE_PORT_SERVEUR, ENTRÉE_PORT_CLIENT, ATTENTE_JOUEURS, CONNECTION }
    public class Jeu : Microsoft.Xna.Framework.GameComponent
    {
        //constantes
        const float INTERVALLE_MAJ = 1f / 60f;
        const int NB_VOITURES_DUMMIES = 10;
        const int LARGEUR_ENTRE_VOITURE = 100;
        const int NB_TOURS = 3;
        const int LARGEUR_DÉPART = 3;
        const int ÉTENDUE = 50;
        const int NB_CHECKPOINTS = 2;
        const int LARGEUR = 10;
        const int LONGUEUR = 7;
        const int SECTION_DE_DÉPART = 10;
        const int NB_MENUS = 8;
        const string IP_HÔTE_LOCAL = "127.0.0.1";
        const int PORT = 5001;
        const string J1 = "JOUEUR 1";
        const string J2 = "JOUEUR 2";
        const int NB_SECONDES_DÉCOMPTE = 5;
        Vector3 RotationInitiale = new Vector3(0, 3.14f, 0);
        List<string> ChoixVoiture = new List<string>() { "GLX_400", "FBX_2010_chevy_volt_deepRed", "volks", "FBX_2008_dodge_charger_blue" };
        bool[] CHECKPOINTS_INITIAUX = new bool[NB_CHECKPOINTS] { false, false };
        int[] SECTIONS_CHECKPOINTS = new int[NB_CHECKPOINTS] { 54, 63 };
        Vector2 ÉTENDUE_TOTALE = new Vector2(200 * 4, 200 * 4);

        //PROPRIÉTÉS
        //propriétés qui gèrent la pause
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
                //Les éléments doivent tous être arrêtés
                NetworkManager.TempsDeCourseJ.EstActif = !pause;
                NetworkManager.SendPrêtJeu(!pause);
                foreach(VoitureDummy v in VoituresDummies)
                {
                    v.Enabled = !pause;
                }
            }
        }

        Menu MenuSélectionnéOption { get; set; }
        ÉtatsJeu ÉtatPrécédentOption { get; set; }

        //Propriétés qui gèrent le gagnant
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
            }
        }

        bool JoueurEstArrivé
        {
            get
            {
                return Bannière.EstÀArrivée(Joueur.Position);
            }
        }

        //propriétés en lien avec le LAN
        Server Serveur { get; set; }
        Réseautique NetworkManager { get; set; }
        bool ConnectionÉtablie { get; set; }
        bool ServeurCréé { get; set; }

        //propriétés des états
        ÉtatsJeu État { get; set; }
        ÉtatsJoueur ÉtatJoueur { get; set; }

        //Propriétés des menus
        List<Menu> Menus { get; set; }
        MenuPrincipal MenuPrincipal { get; set; }
        MenuOption MenuDesOptions { get; set; }
        MenuLan MenuNetwork { get; set; }
        MenuProfile MenuChoixProfile { get; set; }
        MenuIPServeur MenuServeur { get; set; }
        MenuIPClient MenuClient { get; set; }
        MenuPause MenuDePause { get; set; }
        MenuFinPartie MenuFinDePartie { get; set; }

        //propriétés affichables (Joueurs et Environnement)
        Voiture Joueur { get; set; }
        Voiture Ennemi { get; set; }
        TimerDiminue DécompteInitial { get; set; }
        Titre AffNbTours { get; set; }
        List<Section> Sections { get; set; }
        List<Voiture> VoituresDummies { get; set; }

        //Propriétés de la gestion du nombre de tours
        BannièreArrivée Bannière { get; set; }
        BoundingSphere[] CheckPoints { get; set; }
        int NbTours { get; set; }
        bool[] CheckPointsAtteints { get; set; }

        //autre propriété
        InputManager GestionInput { get; set; }

        #region Constructeur et création des menus
        public Jeu(Game game)
            : base(game)
        {
            CréerMenus();
            ServeurCréé = false;
        }
        void CréerMenus()
        {
            MenuPrincipal = new MenuPrincipal(Game);
            MenuDePause = new MenuPause(Game);
            MenuFinDePartie = new MenuFinPartie(Game);
            MenuDesOptions = new MenuOption(Game);
            MenuNetwork = new MenuLan(Game);
            MenuChoixProfile = new MenuProfile(Game);
            MenuServeur = new MenuIPServeur(Game);
            MenuClient = new MenuIPClient(Game);
            Menus = new List<Menu>(NB_MENUS) { MenuPrincipal, MenuDePause, MenuFinDePartie, MenuDesOptions, MenuNetwork, MenuChoixProfile, MenuServeur, MenuClient };
            foreach( Menu m in Menus)
            {
                Game.Components.Add(m);
            }
        }
        #endregion

        #region Initialisation du jeu
        public override void Initialize()
        {
            CheckPointsAtteints = CHECKPOINTS_INITIAUX;
            NbTours = 0;
            ÉtatJoueur = ÉtatsJoueur.SOLO;
            CréerCaméra();
            CréerEnvironnement();
            Game.Components.Add(new AfficheurFPS(Game, "Arial20", Color.White, 1f));
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            État = ÉtatsJeu.MENU_PRINCIPAL;
        }
        private void CréerCaméra()
        {
            //La position de la caméra sera ajustée selon la voiture
            Vector3 positionCaméra = Vector3.Zero;
            Vector3 cibleCaméra = Vector3.Zero;
            CaméraSubjective CaméraJeu = new CaméraSubjective(Game, positionCaméra, cibleCaméra, Vector3.Up, INTERVALLE_MAJ);
            Game.Components.Add(CaméraJeu);
            Game.Services.AddService(typeof(Caméra), CaméraJeu);
        }
        private void CréerEnvironnement()
        {
            Game.Components.Add(new ArrièrePlanDéroulant(Game, "CielÉtoilé", INTERVALLE_MAJ));
            Sections = new List<Section>();

            //Ce sont des sections où nous ne voulons pas de maison
            List<int> pasDeMaison = new List<int>() { 17, 25, 32, 33, 24, 47, 61, 53, 60, 63, 64, 51, 49, 50, 43, 36, 37, 29, 30, 16, 37, 21, 22, 23 };
            
            //Création des sections
            for (int i = 0; i < LARGEUR; ++i)
            {
                for (int j = 0; j < LONGUEUR; ++j)
                {
                    bool maison = !pasDeMaison.Contains(Sections.Count);
                    Section newSection = new Section(Game, new Vector2(ÉTENDUE * i, ÉTENDUE * j), new Vector2(ÉTENDUE, ÉTENDUE), 1f, Vector3.Zero, Vector3.Zero, new Vector3(ÉTENDUE, 25, ÉTENDUE), new string[] { "Herbe", "Sable" }, maison, INTERVALLE_MAJ); //double??
                    Sections.Add(newSection);
                    newSection.Initialize();
                    Game.Components.Add(newSection);
                }
            }
            CréerCheckpoints();
        }
        private void CréerCheckpoints()
        {
            CheckPoints = new BoundingSphere[NB_CHECKPOINTS];
            for (int i = 0; i < NB_CHECKPOINTS; ++i)
            {
                CheckPoints[i] = Sections[SECTIONS_CHECKPOINTS[i]].CréerCheckPoint();
            }
            Bannière = Sections[SECTION_DE_DÉPART].CréerBannière();
            //On veut que la première section soit visible dès le départ
            Sections[SECTION_DE_DÉPART].Visible = true;
        }
        #endregion

        public override void Update(GameTime gameTime)
        {
            GérerTransition();
            GérerÉtat();
            GérerDéconnection(); 
        }

        private void GérerDéconnection()
        {
            //On veut que le joueur arrête de jouer si son concurent s'est déconnecté
            if (ConnectionÉtablie && NetworkManager.DisconnectedT)
            {
                État = ÉtatsJeu.MENU_PRINCIPAL;
                foreach(Menu m in Menus)
                {
                    m.Enabled = false;
                }
                MenuPrincipal.Enabled = true;
                NetworkManager.DisconnectedT = false;
                ConnectionÉtablie = false;
                NetworkManager.Close();
            }
        }

        #region États
        private void GérerÉtat()
        {
            //Ce sont les seuls états où l'ennemi bouge, où il y des collisions et où les joueurs peuvent atteindre des checkpoints.
            if(État == ÉtatsJeu.JEU||État == ÉtatsJeu.GAGNÉ||État == ÉtatsJeu.PERDU)
            {
                GérerÉtatJeu();
            }
        }
        private void GérerÉtatJeu()
        {
            if (ÉtatJoueur != ÉtatsJoueur.SOLO)
            {
                Ennemi.AjusterPosition(NetworkManager.MatriceMondeEnnemi);
            }
            GérerCollisions();
            GérerCheckPoints();
        }
        private void GérerCollisions()
        {
            //Collision entre joueurs
            if (ÉtatJoueur != ÉtatsJoueur.SOLO && Joueur.EstEnCollision2(Ennemi))
            {
                Joueur.Rebondir(Ennemi.Direction, Ennemi.Position);
            }
            //Collision avec objets
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
        private void GérerCheckPoints()
        {
            for(int i = 0; i < NB_CHECKPOINTS; ++i)
            {
                if(CheckpointsPrécédentsAtteints(i) && CheckPoints[i].Contains(Joueur.Position) == ContainmentType.Contains)
                {
                    CheckPointsAtteints[i] = true;
                }
            }
            if (CheckPointsAtteints[NB_CHECKPOINTS - 1] && JoueurEstArrivé)
            {
                ++NbTours;
                CheckPointsAtteints = CHECKPOINTS_INITIAUX;
                AffNbTours.ChangerTexte("Tour : " + (NbTours + 1).ToString());
            }
        }
        bool CheckpointsPrécédentsAtteints(int cpt)
        {
            int i = 0;
            bool checkpointsAtteints = true;
            while(checkpointsAtteints && i < cpt)
            {
                checkpointsAtteints = CheckPointsAtteints[i];
                ++i;
            }
            return checkpointsAtteints;
        }
        #endregion


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
                    ConnectionAuServeur(IP_HÔTE_LOCAL, PORT);
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

        private void GérerTransitionMenuProfile()
        {
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
                    MenuChoixProfile.Pseudonyme = J1;
                }
                else
                {
                    MenuChoixProfile.Pseudonyme = J2;
                }
            }
            NetworkManager.PseudonymeJ = MenuChoixProfile.Pseudonyme;
        }

        private void InitialiserDécompte()
        {
            DécompteInitial = new TimerDiminue(Game, new TimeSpan(0, 0, NB_SECONDES_DÉCOMPTE), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2), "Blanc", true, false, Color.White, 1f);
            Game.Components.Add(DécompteInitial);
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

        private void ConnectionAuServeur(string ip, int port)
        {
            //Si le serveur est déjà créé, on ne veut pas le refaire
            if (!ServeurCréé)
            {
                if (ÉtatJoueur != ÉtatsJoueur.CLIENT)
                {
                    Serveur = new Server(port, ip);
                    Game.Services.AddService(typeof(Server), Serveur);
                }
                NetworkManager = new Réseautique(Game, ip, port);
                NetworkManager.SetEnnemi(MenuDesOptions.GetDifficulté());
                Game.Services.AddService(typeof(Réseautique), NetworkManager);
                ServeurCréé = true;
            }
            else
            {
                NetworkManager.Reset(ip, port);
            }
            ConnectionÉtablie = true;
        }

        private void GérerTransitionConnection()
        {
            if (NetworkManager.enemiConnecté)
            {
                État = ÉtatsJeu.CHOIX_PROFILE;
                MenuServeur.Enabled = false;
                MenuChoixProfile.Enabled = true;
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

        private void GérerTransitionPause()
        {
            switch (MenuDePause.Choix)
            {
                case ChoixMenu.JOUER:
                    MenuDePause.Enabled = false;
                    État = ÉtatsJeu.JEU;
                    Pause = false;
                    break;
                case ChoixMenu.QUITTER:
                    MenuDePause.Enabled = false;
                    État = ÉtatsJeu.MENU_PRINCIPAL;
                    MenuPrincipal.Enabled = true;
                    NetworkManager.SendDisconnect();
                    NetworkManager.Close();
                    ConnectionÉtablie = false;
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
            if (NbTours == NB_TOURS)
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







        private void GérerTransitionDécompte()
        {
            if (!DécompteInitial.EstActif)
            {
                État = ÉtatsJeu.JEU;
                Joueur.EstActif = true;
                DécompteInitial.Enabled = false;
                DécompteInitial.Visible = false;
                NetworkManager.TempsDeCourseJ = new TimerAugmente(Game, new TimeSpan(0), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, 30), "Blanc", true, false, Color.White, INTERVALLE_MAJ);
                Game.Components.Add(NetworkManager.TempsDeCourseJ);
                MenuDesOptions.DésactiverDifficulté();
                AffNbTours = new Titre(Game, "Tour : 1", "Arial20", new Vector2(Game.Window.ClientBounds.Width / 15, Game.Window.ClientBounds.Height / 18), "Blanc", false, Color.White);
                Game.Components.Add(AffNbTours);
            }
        }




        #endregion



        #region initialisation du jeu
        private void InitialiserLeJeu()
        {
            Reset();
            CréerVoitureDummy();
            Vector2 v = Sections[10].ObtenirPointDépart();
            if (ÉtatJoueur != ÉtatsJoueur.SOLO)
            {
                CréerEnnemi(v);
            }
            CréerJoueur(v);  
        }

        private void CréerVoitureDummy()
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



        private void CréerEnnemi(Vector2 Départ)
        {
            Vector3 pos = ÉtatJoueur == ÉtatsJoueur.SERVEUR ? new Vector3(Départ.X - LARGEUR_DÉPART, 0, Départ.Y) : new Vector3(Départ.X + LARGEUR_DÉPART, 0, Départ.Y);
            Ennemi = new Voiture(Game, ChoixVoiture[NetworkManager.ChoixVoitureE], 0.01f, Vector3.Zero, pos, INTERVALLE_MAJ); //Get choix de voiture??
            Ennemi.EstActif = false;
            Game.Components.Add(Ennemi);
        }

        private void CréerJoueur(Vector2 Départ)
        {
            Vector3 pos = ÉtatJoueur != ÉtatsJoueur.CLIENT ? new Vector3(LARGEUR_DÉPART + Départ.X, 0, Départ.Y) : new Vector3(Départ.X - LARGEUR_DÉPART, 0, Départ.Y);
            Joueur = new Voiture(Game, ChoixVoiture[NetworkManager.ChoixVoitureJ], 0.01f, RotationInitiale, pos, INTERVALLE_MAJ);
            Joueur.EstActif = false;
            Game.Components.Add(Joueur);
            NetworkManager.SendPosIni(Joueur.Position); //fonctionne pas....
        }


        #endregion



    }
}
