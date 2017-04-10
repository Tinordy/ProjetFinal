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

    public class AfficheurGagnants : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Réseautique NetworkManager { get; set; }
        Titre Gagnant { get; set; }
        Titre Perdant { get; set; }
        public AfficheurGagnants(Game game)
            : base(game)
        {

        }
        public override void Initialize()
        {
            base.Initialize();
        }
        void Activer()
        {
            NetworkManager = Game.Services.GetService(typeof(Réseautique)) as Réseautique; //on le fait plusieurs fois... PAS GRAVE?
            //Game.Components.Remove(Gagnant);
            //Game.Components.Remove(Perdant);
     
            Gagnant = new Titre(Game, NetworkManager.PseudonymeJ + '-' + NetworkManager.TempsDeCourseJ.ValeurTimer.ToString("mm':'ss','ff"), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2), "Blanc");
            Game.Components.Add(Gagnant);
            Gagnant.DrawOrder = 2;
            Perdant = new Titre(Game, NetworkManager.PseudonymeE + '-' + NetworkManager.TempsDeCourseE, "Arial", new Vector2(Game.Window.ClientBounds.Width/2, 3*Game.Window.ClientBounds.Height/5), "Blanc");
            Game.Components.Add(Perdant);
            Perdant.DrawOrder = 2;

        }
        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            if (Enabled)
            {
                Activer(); //moyennement legit
            }
            else
            {
                if(Gagnant != null)
                {
                    Game.Components.Remove(Gagnant);
                    Game.Components.Remove(Perdant);
                }
            }
            base.OnEnabledChanged(sender, args);
        }
    }
}
