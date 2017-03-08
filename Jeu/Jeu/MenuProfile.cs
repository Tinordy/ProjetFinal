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
using System.Windows.Forms;





namespace AtelierXNA
{
    public class MenuProfile : Menu
    {
        int Voiture { get; set; }
        string Pseudonyme { get; set; }
        public MenuProfile(Game game)
            : base(game)
        { }

        public override void Initialize()
        {
            base.Initialize();
            Composantes.Add(new Arri�rePlan(Game, "MenuOption"));
            Composantes.Add(new Titre(Game, "Pseudonyme: ", "Arial", new Vector2(Game.Window.ClientBounds.Width / 4, Game.Window.ClientBounds.Height / 4), "Blanc"));
            Composantes.Add(new Entr�eDeTexte(Game, new Vector2(3 * Game.Window.ClientBounds.Width / 4, Game.Window.ClientBounds.Height / 4), "Arial20"));
            List<string> noms = new List<string>();
            noms.Add("Ciel�toil�");
            noms.Add("Dragon");
            noms.Add("brique1");
            Composantes.Add(new D�fileurSprite(Game,noms, new Rectangle(0,Game.Window.ClientBounds.Height/3, Game.Window.ClientBounds.Width,2* Game.Window.ClientBounds.Height/3), 0.01f));
            Composantes.Add(new BoutonDeCommande(Game, "D�marrer", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 2, 8 * Game.Window.ClientBounds.Height / 9), true, D�marrer, 0.01f));

            Activer();
        }

        private void D�marrer()
        {
            Choix = ChoixMenu.JOUER;
        }
        //methode pour pseu et voiture... att pseu vide?
    }
}
