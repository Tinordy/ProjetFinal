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
    public class MenuPrincipal : Menu
    {
        public MenuPrincipal(Game game)
            : base(game)
        { }

        public override void Initialize()
        {
            base.Initialize();
            Composantes.Add(new ArrièrePlan(Game, "FondMenu"));
            Composantes.Add(new BoutonDeCommande(Game, "Jouer", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 5, Game.Window.ClientBounds.Height / 5), true, Jouer, 0.01f));
            Composantes.Add(new BoutonDeCommande(Game, "Options", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 5, 2 * Game.Window.ClientBounds.Height / 5), true, Option, 0.01f));
            Composantes.Add(new BoutonDeCommande(Game, "Quitter", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 5, 4 * Game.Window.ClientBounds.Height / 5), true, Quitter, 0.01f));

            Activer();
        }
        public void Jouer()
        {
            Choix = ChoixMenu.JOUER;
        }
        public void Option()
        {
            Choix = ChoixMenu.OPTION;
        }
        public void Quitter()
        {
            Choix = ChoixMenu.QUITTER;
        }

        //dans menu, ou enlever?
        public void DésactiverBoutons()
        {
            foreach (BoutonDeCommande bouton in Composantes.Where(c => c is BoutonDeCommande))
            {
                //bouton.EstActif = false;
                bouton.EstActif = !bouton.EstActif;
                if (bouton.EstActif)
                {
                    Choix = ChoixMenu.EN_ATTENTE; // PO LEGIT!!!
                }
            }

        }
    }
}
