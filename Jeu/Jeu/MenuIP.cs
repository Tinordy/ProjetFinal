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

   public class MenuIP : Menu
   {
      public string IP { get; protected set; }
      public int Port { get; private set; }
      Entr�eDeTexte LecteurPort { get; set; }

      public MenuIP(Game game)
          : base(game)
      {
      }

      public override void Initialize()
      {
         LecteurPort = new Entr�eDeTexte(Game, new Vector2(3 * Game.Window.ClientBounds.Width / 5, Game.Window.ClientBounds.Height / 3), "Arial", 6);

         base.Initialize();
         Composantes.Add(new Arri�rePlan(Game, "Carte"));
         Composantes.Add(new Titre(Game, "Port : ", "Arial", new Vector2(Game.Window.ClientBounds.Width / 5, Game.Window.ClientBounds.Height / 3), "Blanc"));
         Composantes.Add(LecteurPort);
         Composantes.Add(new Titre(Game, "IP :", "Arial", new Vector2(Game.Window.ClientBounds.Width / 5, 2 * Game.Window.ClientBounds.Height / 3), "Blanc"));
         Composantes.Add(new BoutonDeCommande(Game, "OK", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 2, 4 * Game.Window.ClientBounds.Height / 5), true, Lire, 0.01f));



      }
      protected virtual void Lire()
      {
         //g�rer exceptions
         Port = int.Parse(LecteurPort.ObtenirEntr�e());
         Port = int.Parse(LecteurPort.ObtenirEntr�e());
         Choix = ChoixMenu.CONNECTION;
      }
   }
}
