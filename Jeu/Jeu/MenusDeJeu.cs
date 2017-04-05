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

    enum ÉtatsMenus { MENU_PRINCIPAL, MENU_OPTION, CHOIX_LAN, CHOIX_PROFILE, ENTRÉE_PORT_SERVEUR, ENTRÉE_PORT_CLIENT, MENU_PAUSE, CONNECTION, PAUSE, STAND_BY, ATTENTE_JOUEURS }
    public class MenusDeJeu : Microsoft.Xna.Framework.GameComponent
    {
        ÉtatsMenus État { get; set; }
        MenuPrincipal MenuPrincipal { get; set; }

        public MenusDeJeu(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {

            base.Initialize();
        }

        private void CréerMenus()
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
            GérerTransition();
            base.Update(gameTime);
        }

        private void GérerTransition()
        {
            switch (État)
            {
                case ÉtatsMenus.MENU_PRINCIPAL:
                    GérerTransitionMenuPrincipal();
                    break;
            }
        }

        private void GérerTransitionMenuPrincipal()
        {
            throw new NotImplementedException();
        }
    }
}
