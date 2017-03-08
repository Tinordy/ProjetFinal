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
    enum ÉtatsJeu { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, JEU, CHOIX_PROFILE }
    public class Jeu : Microsoft.Xna.Framework.GameComponent
    {
        ÉtatsJeu État { get; set; }
        MenuPrincipal MenuPrincipal { get; set; }
        MenuOption MenuDesOptions { get; set; }
        MenuLan MenuNetwork { get; set; }
        MenuProfile MenuChoixProfile { get; set; }

        public Jeu(Game game)
            : base(game)
        {
            CréerMenuPrincipal();
            CréerMenuLan();
            CréerMenuOption();
            CréerMenuProfile();
        }

        public override void Initialize()
        {
            État = ÉtatsJeu.MENU_PRINCIPAL;
            MenuPrincipal.Enabled = true;

        }

        public override void Update(GameTime gameTime)
        {
            GérerTransition();
        }

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
            }

        }

        private void GérerTransitionMenuProfile()
        {
            switch(MenuChoixProfile.Choix)
            {
                case ChoixMenu.EN_ATTENTE:
                    break;
                case ChoixMenu.JOUER:
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
                case ChoixMenu.PROFILE_SOLO:
                    État = ÉtatsJeu.CHOIX_PROFILE;
                    MenuNetwork.Enabled = false;
                    MenuChoixProfile.Enabled = true;
                    break;
                case ChoixMenu.PROFILE_MULTI:
                    État = ÉtatsJeu.CHOIX_PROFILE;
                    MenuNetwork.Enabled = false;
                    MenuChoixProfile.Enabled = true;
                    ConnectionAuServeur();
                    break;
                case ChoixMenu.SERVEUR:
                    État = ÉtatsJeu.CHOIX_PROFILE;
                    MenuNetwork.Enabled = false;
                    MenuChoixProfile.Enabled = true;
                    CréerServeur();
                    break;
                case ChoixMenu.RETOUR:
                    État = ÉtatsJeu.MENU_PRINCIPAL;
                    MenuNetwork.Enabled = false;
                    MenuPrincipal.Enabled = true;
                    break;
            }
        }

        private void CréerServeur()
        {
            //dave
        }
        private void ConnectionAuServeur()
        {
            //dave
        }
        private void DémarrerLeJeu()
        {
            //démarre le jeu peu importe si c'est un simple jouer ou multijoueur... dérivée de la classe jeu?
        }
        #region Création Des Menus
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
        #endregion

    }
}
