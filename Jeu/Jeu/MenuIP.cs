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

   public abstract class MenuIP : Menu
   {
      public string IP { get; protected set; }
      public int Port { get; private set; }
      EntréeDeTexte LecteurPort { get; set; }
      int IndexSélectionnable
      {
         get
         {
            return ObjetsSélectionnables.FindIndex(o => o.EstSélectionné);
         }
      }
        protected List<ISélectionnable> ObjetsSélectionnables { get; set; } //prop..? :(
        //protected List<ISélectionnable> ObjetsSélectionnables
        //{
        //    get
        //    {
        //        List<ISélectionnable> liste = new List<ISélectionnable>();
        //        foreach (ISélectionnable composante in Composantes.Where(c => c is ISélectionnable))
        //        {
        //            liste.Add(composante);
        //        }
        //        return liste;
        //    }
        //}
        InputManager GestionInput { get; set; }

      public MenuIP(Game game)
          : base(game)
      {
      }

      public override void Initialize()
      {
         ObjetsSélectionnables = new List<ISélectionnable>();
         LecteurPort = new EntréeDeTexte(Game, new Vector2(3 * Game.Window.ClientBounds.Width / 5, Game.Window.ClientBounds.Height / 3), "Arial", 6);
         ObjetsSélectionnables.Add(LecteurPort);
         base.Initialize();
         Composantes.Add(new ArrièrePlan(Game, "Carte"));
         Composantes.Add(new Titre(Game, "Port : ", "Arial", new Vector2(Game.Window.ClientBounds.Width / 5, Game.Window.ClientBounds.Height / 3), "Blanc"));
         Composantes.Add(LecteurPort);
         Composantes.Add(new Titre(Game, "IP :", "Arial", new Vector2(Game.Window.ClientBounds.Width / 5, 2 * Game.Window.ClientBounds.Height / 3), "Blanc"));
         Composantes.Add(new BoutonDeCommande(Game, "OK", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 2, 4 * Game.Window.ClientBounds.Height / 5), true, Lire, 0.01f));
         Composantes.Add(new BoutonDeCommande(Game, "Retour", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 5, 8 * Game.Window.ClientBounds.Height / 9), true, Retour, 0.01f));
         GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;

      }

      private void Retour()
      {
         Choix = ChoixMenu.QUITTER;
      }
      public override void Update(GameTime gameTime)
      {
         if (GestionInput.EstNouvelleTouche(Keys.Tab) && IndexSélectionnable != -1)
         {
            int index = IndexSélectionnable;
            ObjetsSélectionnables[index].EstSélectionné = false;
            //IndexSélectionnable = (IndexSélectionnable + 1) % ObjetsSélectionnables.Count;
            ObjetsSélectionnables[(index + 1) % ObjetsSélectionnables.Count].EstSélectionné = true;
         }
      }
      protected virtual void Lire()
      {
         //gérer exceptions
         Port = int.Parse(LecteurPort.ObtenirEntrée());
         Port = int.Parse(LecteurPort.ObtenirEntrée());
         Choix = ChoixMenu.CONNECTION;
      }
   }
}
