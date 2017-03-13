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
using System.Net;


namespace AtelierXNA
{

   public class MenuIPServeur : MenuIP
   {
      public MenuIPServeur(Game game)
          : base(game)
      {

      }
      public override void Initialize()
      {
         string HostName = Dns.GetHostName();
         IP = Dns.GetHostAddresses(HostName)[1].ToString();
         base.Initialize();
         Composantes.Add(new Titre(Game, IP, "Arial", new Vector2(3 * Game.Window.ClientBounds.Width / 5, 2 * Game.Window.ClientBounds.Height / 3), "Blanc"));
         Activer();
      }
   }
}
