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

    enum �tatsMenus { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, CHOIX_PROFILE, ENTR�E_PORT_SERVEUR, ENTR�E_PORT_CLIENT, MENU_PAUSE, CONNECTION, PAUSE, STAND_BY, ATTENTE_JOUEURS }
    public class MenusDeJeu : Microsoft.Xna.Framework.GameComponent
    {
        �tatsMenus �tat { get; set; }
        MenuPrincipal MenuPrincipal { get; set; }

        public MenusDeJeu(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {

            base.Initialize();
        }

        private void Cr�erMenus()
        {
            MenuPrincipal = new MenuPrincipal(Game);
            //MenuDesOptions = new MenuOption(Game);
            //MenuNetwork = new MenuLan(Game);
            //MenuChoixProfile = new MenuProfile(Game);
            //MenuServeur = new MenuIPServeur(Game);
            //MenuClient = new MenuIPClient(Game);
            //MenuDePause = new MenuPause(Game);

            Game.Components.Add(MenuPrincipal);
            //Game.Components.Add(MenuDesOptions);
            //Game.Components.Add(MenuNetwork);
            //Game.Components.Add(MenuChoixProfile);
            //Game.Components.Add(MenuServeur);
            //Game.Components.Add(MenuClient);
            //Game.Components.Add(MenuDePause);
        }

        public override void Update(GameTime gameTime)
        {
            G�rerTransition();
            base.Update(gameTime);
        }

        private void G�rerTransition()
        {
            switch (�tat)
            {
                case �tatsMenus.MENU_PRINCIPAL:
                    G�rerTransitionMenuPrincipal();
                    break;
            }
        }

        private void G�rerTransitionMenuPrincipal()
        {
            throw new NotImplementedException();
        }
    }
}
