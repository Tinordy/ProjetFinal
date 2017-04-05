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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class MenuPause : Menu
    {
        //public bool estActf;
        //public bool EstActif
        //{
        //    get
        //    {
        //        return estActf;
        //    }
        //    set
        //    {
        //        if(value != estActf)
        //        {
        //            foreach(BoutonDeCommande c in Composantes)
        //            {

        //            }
        //        }
        //    }
        //}
        public MenuPause(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }
        public override void Initialize()
        {
            base.Initialize();
            Composantes.Add(new ArrièrePlan(Game, "FondPause"));
            Composantes.Add(new BoutonDeCommande(Game, "Continuer", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 4, Game.Window.ClientBounds.Height / 5), true, Continuer, 0.01f));
            Composantes.Add(new BoutonDeCommande(Game, "Rage Quit", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 4, 4 * Game.Window.ClientBounds.Height / 5), true, Quitter, 0.01f));
            Composantes.Add(new BoutonDeCommande(Game, "Options", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 4, 2 * Game.Window.ClientBounds.Height / 5), true, Option, 0.01f));

            Activer();
        }

        private void Option()
        {
            Choix = ChoixMenu.OPTION;
            //Ouvrir menu options... état revenir en arrière..? :O
        }
        private void Quitter()
        {
            Choix = ChoixMenu.QUITTER;
            //Retour au menu principal, déconnexion des joueurs... demander validation?
        }
        private void Continuer()
        {
            Choix = ChoixMenu.JOUER;
            //transition dans menu, faire un décompte pour continuer
        }
    }
}
