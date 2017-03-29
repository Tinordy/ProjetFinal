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
   public class Section : Terrain
   {
      const float INTERVALLE_MAJ_STANDARD = 1f / 60f;
      Vector2 Étendue { get; set; }
      Vector2 Extrémité { get; set; }
      double NormeÉtendue { get; set; }
      Maison Test { get; set; }

      List<DrawableGameComponent> Components { get; set; }
      public Section(Game game, Vector2 origine, Vector2 étendue2, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, Vector3 étendue,
                     string[] nomsTexturesTerrain, float intervalleMAJ)
          : base(game, origine, homothétieInitiale, rotationInitiale, positionInitiale, étendue, nomsTexturesTerrain, intervalleMAJ)
      {
         //Étendue = étendue2;
         Étendue = new Vector2(étendue.X, étendue.Z);

      }

      public override void Initialize()
      {
         Components = new List<DrawableGameComponent>();
         Extrémité = Coin + Étendue;
         Test = new Maison(Game, 10f, Vector3.Zero, new Vector3(Coin.X/* + Étendue.X*/, 0, Coin.Y/* - Étendue.Y*/), new Vector3(2, 2, 2), "brique1", "roof", 0.01f);
         Game.Components.Add(Test);
         Components.Add(Test);
         NormeÉtendue = Math.Sqrt(Math.Pow(Étendue.X, 2) + Math.Pow(Étendue.Y, 2));
         CréerPiste();
         base.Initialize();
      }
      //lol
      private void CréerPiste()
      {
         Game.Components.Add(new PisteSectionnée(Game, 1f, Vector3.Zero, Vector3.Zero, INTERVALLE_MAJ_STANDARD, 20000, 20000, Coin, Étendue));
      }
      protected override void OnVisibleChanged(object sender, EventArgs args)
      {
         foreach(DrawableGameComponent c in Components)
         {
            c.Visible = Visible;
         }
      }

      //public void AddComponent(GameComponent x)
      //{
      //    Components.Add(x);
      //}
   }
}
