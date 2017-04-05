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
      Entr�eDeTexte LecteurPort { get; set; }
      int IndexS�lectionnable
      {
         get
         {
            return ObjetsS�lectionnables.FindIndex(o => o.EstS�lectionn�);
         }
      }
        protected List<IS�lectionnable> ObjetsS�lectionnables { get; set; } //prop..? :(
        //protected List<IS�lectionnable> ObjetsS�lectionnables
        //{
        //    get
        //    {
        //        List<IS�lectionnable> liste = new List<IS�lectionnable>();
        //        foreach (IS�lectionnable composante in Composantes.Where(c => c is IS�lectionnable))
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
         ObjetsS�lectionnables = new List<IS�lectionnable>();
         LecteurPort = new Entr�eDeTexte(Game, new Vector2(3 * Game.Window.ClientBounds.Width / 5, Game.Window.ClientBounds.Height / 3), "Arial", 6);
         ObjetsS�lectionnables.Add(LecteurPort);
         base.Initialize();
         Composantes.Add(new Arri�rePlan(Game, "Carte"));
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
         if (GestionInput.EstNouvelleTouche(Keys.Tab) && IndexS�lectionnable != -1)
         {
            int index = IndexS�lectionnable;
            ObjetsS�lectionnables[index].EstS�lectionn� = false;
            //IndexS�lectionnable = (IndexS�lectionnable + 1) % ObjetsS�lectionnables.Count;
            ObjetsS�lectionnables[(index + 1) % ObjetsS�lectionnables.Count].EstS�lectionn� = true;
         }
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
