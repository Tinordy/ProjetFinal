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

namespace AtelierXNA
{
    //GROSSE CLASSE MENUSDEJEU???
    //
    enum �tatsJeu { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, JEU, CHOIX_PROFILE, ENTR�E_PORT_SERVEUR, ENTR�E_PORT_CLIENT, MENU_PAUSE, CONNECTION, ATTENTE_JOUEURS, D�COMPTE, PAUSE, STAND_BY, GAGN�, PERDU, FIN_DE_PARTIE }
    enum �tatsJoueur { SOLO, SERVEUR, CLIENT }
    public class Jeu : Microsoft.Xna.Framework.GameComponent
    {
        List<string> UsedIP { get; set; } //LEGIT?
        bool Connect�Test { get; set; }
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
                TempsDeCourse.EstActif = !pause;
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
                NetworkManager.SendTermin�(gagn�);
                if (gagn�)
                {
                    �tat = �tatsJeu.GAGN�;
                    TempsDeCourse.EstActif = false;
                }
                else
                {
                    �tat = �tatsJeu.PERDU;
                }
                //diff�rent, fin de partie
                Game.Components.Add(new Titre(Game, �tat.ToString(), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2), "Blanc"));
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
        �tatsJeu �tat { get; set; }
        �tatsJoueur �tatJoueur { get; set; }
        MenuPrincipal MenuPrincipal { get; set; }
        MenuOption MenuDesOptions { get; set; }
        MenuLan MenuNetwork { get; set; }
        MenuProfile MenuChoixProfile { get; set; }
        MenuIPServeur MenuServeur { get; set; }
        MenuIPClient MenuClient { get; set; }
        MenuPause MenuDePause { get; set; }
        Menu MenuS�lectionn�Option { get; set; }
        �tatsJeu �tatPr�c�dentOption { get; set; }

        Server Serveur { get; set; }
        R�seautique NetworkManager { get; set; }
        Voiture Joueur { get; set; }
        Voiture Ennemi { get; set; }
        List<Section> Sections { get; set; }
        TimerDiminue D�compteInitial { get; set; }
        TimerAugmente TempsDeCourse { get; set; }
        Vector2 �tendueTotale { get; set; }
        InputManager GestionInput { get; set; }
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
            �tatJoueur = �tatsJoueur.SOLO;
            Cr�erCam�ra();
            Cr�erEnvironnement();
            Game.Components.Add(new AfficheurFPS(Game, "Arial20", Color.White, 1f));

            UsedIP = new List<string>();
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            �tat = �tatsJeu.MENU_PRINCIPAL;
            MenuPrincipal.Enabled = true;
        }

        public override void Update(GameTime gameTime)
        {
            G�rerTransition();
            G�rer�tat();
            G�rerD�connection();
        }

        private void G�rerD�connection()
        {
            //d�buter seulement lorque c'est n�cessaire (apr�s menus...)

            //if (�tatJoueur != �tatsJoueur.SOLO && Connect�Test && !NetworkManager.enemiConnect�)
            //{
            //    �tat = �tatsJeu.MENU_PRINCIPAL;
            //    MenuPrincipal.Enabled = true;
            //}
        }

        private void G�rer�tat()
        {
            switch (�tat)
            {
                case �tatsJeu.JEU:
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
            }

        }

        private void G�rerTransitionPerdu()
        {
            if (JoueurEstArriv�)
            {
                �tat = �tatsJeu.FIN_DE_PARTIE;
                TempsDeCourse.EstActif = false;
                Joueur.EstActif = false;
                NetworkManager.SendTermin�(true);
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
                if (JoueurEstArriv�)
                {
                    if (�tatJoueur == �tatsJoueur.SOLO)
                    {
                        Gagn� = TempsDeCourse.ValeurTimer < new TimeSpan(0, 0, 10); //obtenir vraie valeur
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
                TempsDeCourse = new TimerAugmente(Game, new TimeSpan(0), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, 30), "Blanc", true, 0.01f);
                Game.Components.Add(TempsDeCourse);
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

        private void G�rerTransitionConnection()
        {
            if (Serveur.connectedClients == 2)
            {
                �tat = �tatsJeu.CHOIX_PROFILE;
                MenuServeur.Enabled = false;
                MenuChoixProfile.Enabled = true;
                Connect�Test = true;
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
                        NetworkManager.SendPr�tJeu(true);
                    }
                    �tat = �tatsJeu.ATTENTE_JOUEURS;
                    break;
                case ChoixMenu.JOUER:
                    //retirer tous les menus des components?
                    �tat = �tatsJeu.D�COMPTE;
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
        private void ConnectionAuServeur(string ip, int port)
        {
            if(UsedIP.FindIndex(s => s ==ip) == -1) //marche simple joueur, pt pas multijoueur?
            {
                Serveur = new Server(port, ip);
                Game.Services.AddService(typeof(Server), Serveur);
                NetworkManager = new R�seautique(Serveur, ip, port);
                Game.Services.AddService(typeof(R�seautique), NetworkManager);
                UsedIP.Add(ip);
            }
        }
        #region initialisation du jeu
        private void InitialiserLeJeu()
        {
            Reset();
            Cr�erJoueur();
            if (�tatJoueur != �tatsJoueur.SOLO)
            {
                Cr�erEnnemi();
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
        }

        private void Cr�erEnvironnement()
        {
            Game.Components.Add(new Arri�rePlanD�roulant(Game, "Ciel�toil�", 0.01f));
            Sections = new List<Section>();

            �tendueTotale = new Vector2(200 * 4, 200 * 4); //envoyer � voiture?
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    Section newSection = new Section(Game, new Vector2(200 * i, 200 * j), new Vector2(200, 200), 1f, Vector3.Zero, Vector3.Zero, new Vector3(200, 25, 200), new string[] { "Herbe", "Sable" }, 0.01f); //double??
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
            Ennemi = new Voiture(Game, "GLX_400", 0.1f, Vector3.Zero, NetworkManager.PositionEnnemi, 0.01f); //Get choix de voiture??
            Ennemi.EstActif = false;
            Game.Components.Add(Ennemi);
        }

        private void Cr�erJoueur()
        {
            Joueur = new Voiture(Game, "GLX_400", 0.1f, Vector3.Zero, new Vector3(100, 0, 50), 0.01f); //mettre choix?
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

        #region Cr�ation Des Menus //juste les n�cess?
        void Cr�erMenuPrincipal()
        {
            MenuPrincipal = new MenuPrincipal(Game);
            Game.Components.Add(MenuPrincipal);
            //lol autre place
            MenuDePause = new MenuPause(Game);
            Game.Components.Add(MenuDePause);
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
