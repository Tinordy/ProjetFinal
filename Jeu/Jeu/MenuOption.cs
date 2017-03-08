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
    public class MenuOption : Menu
    {
        public MenuOption(Game game)
            : base(game)
        { }

        public override void Initialize()
        {
            base.Initialize();
            Composantes.Add(new Sprite(Game, "menuOption", new Rectangle(Game.Window.ClientBounds.Width / 8, Game.Window.ClientBounds.Height / 8, 3 * Game.Window.ClientBounds.Width / 4, 3 * Game.Window.ClientBounds.Height / 4)));
            Composantes.Add(new Titre(Game, "Options", "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 4), "Blanc"));
            Composantes.Add(new BoutonDeCommande(Game, "Retour", "Arial", "BoutonVert", "BoutonNoir", new Vector2(3 * Game.Window.ClientBounds.Width / 4, 3 * Game.Window.ClientBounds.Height / 4), true, Retour, 0.01f));

            Activer();
        }
        public void Retour()
        {
            Choix = ChoixMenu.RETOUR;
        }
    }
}
