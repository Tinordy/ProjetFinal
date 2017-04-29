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
        string[] Pseudonymes { get; set; }
        TimeSpan[] Temps { get; set; }
        int IndexGagnant { get; set; }
        Titre Gagnant { get; set; }
        Titre Perdant { get; set; }
        Titre Félicitation { get; set; }
        public AfficheurGagnants(Game game)
            : base(game)
        {

        }
        public override void Initialize()
        {
            Pseudonymes = new string[2];
            Temps = new TimeSpan[2];
            base.Initialize();
        }
        void Activer()
        {
            NetworkManager = Game.Services.GetService(typeof(Réseautique)) as Réseautique; //on le fait plusieurs fois... PAS GRAVE?
            //Game.Components.Remove(Gagnant);
            //Game.Components.Remove(Perdant);

            Pseudonymes[0] = NetworkManager.PseudonymeJ;
            Pseudonymes[1] = NetworkManager.PseudonymeE;
            Temps[0] = NetworkManager.TempsDeCourseJ.ValeurTimer;
            Temps[1] = NetworkManager.TempsDeCourseE;
            IndexGagnant = Convert.ToInt32(Temps[0] > Temps[1]);

            Gagnant = new Titre(Game, Pseudonymes[IndexGagnant] + " - " + Temps[IndexGagnant].ToString("mm':'ss','ff"), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2), "Blanc");
            Game.Components.Add(Gagnant);
            Gagnant.DrawOrder = 2;

            Perdant = new Titre(Game, Pseudonymes[(IndexGagnant + 1) % 2] + " - " + Temps[(IndexGagnant + 1) % 2].ToString("mm':'ss','ff"), "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, 3 * Game.Window.ClientBounds.Height / 5), "Blanc");
            Game.Components.Add(Perdant);
            Perdant.DrawOrder = 2;

            Félicitation = new Titre(Game, "Félicitations " + Pseudonymes[IndexGagnant] + '!', "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, 5 * Game.Window.ClientBounds.Height / 6), "Blanc");
            Game.Components.Add(Félicitation);
            Félicitation.DrawOrder = 2;

        }
        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            if (Enabled)
            {
                Activer(); //moyennement legit
            }
            else
            {
                if(Gagnant != null) //ouin...
                {
                    Game.Components.Remove(Gagnant);
                    Game.Components.Remove(Perdant);
                    Game.Components.Remove(Félicitation);
                }
            }
            base.OnEnabledChanged(sender, args);
        }
    }
}
