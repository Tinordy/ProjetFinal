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
   public class MenuIPClient : MenuIP
   {
      EntréeDeTexte LecteurIP { get; set; }
      public MenuIPClient(Game game)
          : base(game)
      {

      }

      public override void Initialize()
      {
         LecteurIP = new EntréeDeTexte(Game, new Vector2(3 * Game.Window.ClientBounds.Width / 5, 2 * Game.Window.ClientBounds.Height / 3), "Arial", 14);
         base.Initialize();
         Composantes.Add(LecteurIP);

         Activer();
      }
      protected override void Lire()
      {
         base.Lire();
         IP = LecteurIP.ObtenirEntrée();
      }
   }
}
