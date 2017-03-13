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
    public class MenuLan : Menu
    {
        public MenuLan(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            Composantes.Add(new ArrièrePlan(Game, "Neige"));
            Composantes.Add(new BoutonDeCommande(Game, "Annuler", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 5, 4 * Game.Window.ClientBounds.Height / 5), true, Annuler, 0.01f));
            Composantes.Add(new Titre(Game, "Solo", "Arial", new Vector2(Game.Window.ClientBounds.Width / 5, Game.Window.ClientBounds.Height / 5), "Blanc"));
            Composantes.Add(new BoutonDeCommande(Game, "Démarrer", "Arial20", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 5, 2 * Game.Window.ClientBounds.Height / 5), true, JouerSolo, 0.01f));
            Composantes.Add(new Titre(Game, "MultiJoueur", "Arial", new Vector2(4 * Game.Window.ClientBounds.Width / 5, Game.Window.ClientBounds.Height / 5), "Blanc"));
            Composantes.Add(new BoutonDeCommande(Game, "Rejoindre", "Arial20", "BoutonVert", "BoutonNoir", new Vector2(4 * Game.Window.ClientBounds.Width / 5, 1.8f * Game.Window.ClientBounds.Height / 5), true, Rejoindre, 0.01f));
            Composantes.Add(new BoutonDeCommande(Game, "Héberger", "Arial20", "BoutonVert", "BoutonNoir", new Vector2(4 * Game.Window.ClientBounds.Width / 5, 2.2f * Game.Window.ClientBounds.Height / 5), true, Héberger, 0.01f));


            Activer();
        }

        private void Héberger()
        {
            Choix = ChoixMenu.SERVEUR;
        }

        private void JouerSolo()
        {
            Choix = ChoixMenu.SOLO;
        }

        public void Annuler()
        {
            Choix = ChoixMenu.RETOUR;
        }
        public void Rejoindre()
        {
            Choix = ChoixMenu.REJOINDRE;
        }
    }
}
