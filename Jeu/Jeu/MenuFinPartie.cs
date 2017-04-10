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
    public class MenuFinPartie : Menu
    {
        AfficheurGagnants AffGagnants { get; set; }
        public MenuFinPartie(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            AffGagnants = new AfficheurGagnants(Game);
            base.Initialize();
            Composantes.Add(new ArrièrePlan(Game, "Herbe"));
            Composantes.Add(new BoutonDeCommande(Game, "Rejouer", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width/4, Game.Window.ClientBounds.Height/5), true, Rejouer, 0.01f));
            Composantes.Add(new BoutonDeCommande(Game, "Quitter", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 4, 2 * Game.Window.ClientBounds.Height / 5), true, Quitter, 0.01f));
            Composantes.Add(AffGagnants);

            Activer();
        }

        private void Quitter()
        {
            Choix = ChoixMenu.QUITTER;
        }

        private void Rejouer() //SEULEMENT ACTIF POUR SERVEUR!!!!!
        {
            Choix = ChoixMenu.JOUER;
        }
        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            base.OnEnabledChanged(sender, args);
            AffGagnants.Enabled = Enabled;
        }
    }
}
