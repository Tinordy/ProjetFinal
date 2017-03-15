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
   public class Voiture : ObjetDeBase
   {
      const float INCR�MENT_ROTATION = (float)Math.PI / 120;

      float IntervalleMAJ { get; set; }
      float Temps�coul�DepuisMAJ { get; set; }
      InputManager GestionInput { get; set; }
      Vector3 Direction { get; set; }
      bool ChangementEffectu� { get; set; }
      Cam�ra Cam�ra { get; set; }

      //Rotation... changement effextu�? prp auto

      public Voiture(Game game, string nomMod�le, float �chelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ)
          : base(game, nomMod�le, �chelleInitiale, rotationInitiale, positionInitiale)
      {
         IntervalleMAJ = intervalleMAJ;
      }

      public override void Initialize()
      {
         //ok?
         Direction = new Vector3(-1, 0, 0);
         GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
         Cam�ra = Game.Services.GetService(typeof(Cam�ra)) as Cam�ra;
         base.Initialize();//keep
      }

      public override void Update(GameTime gameTime)
      {
         float temps�coul� = (float)gameTime.ElapsedGameTime.TotalSeconds;
         Temps�coul�DepuisMAJ += temps�coul�;
         if (Temps�coul�DepuisMAJ >= IntervalleMAJ)
         {
            G�rerVolant();
            //D�placerCam�ra();
            EffectuerTransformations();
            Temps�coul�DepuisMAJ = 0;
         }
      }
      //marche po...
      //private void D�placerCam�ra()
      //{
      //   Vector3 anciennePositionCam�ra = Cam�ra.Position;
      //   Vector3 nouvellePositionCam�ra = Position - Direction * 500 + new Vector3(0, 3, 0); //const hauteur de la cam�ra
      //   Vector3 d�placementCam�ra = (nouvellePositionCam�ra - anciennePositionCam�ra) / 20;

      //   Cam�ra.D�placer(anciennePositionCam�ra + d�placementCam�ra/*new Vector3(d�placementCam�ra.X, 0, d�placementCam�ra.Z)*/, Position, Vector3.Up);
      //}

      private void EffectuerTransformations()
      {
         if (ChangementEffectu�)
         {
            Monde = Matrix.CreateScale(�chelle) * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) * Matrix.CreateTranslation(Position);
            ChangementEffectu� = false;
         }
      }

      private void G�rerVolant()
      {
         if (GestionInput.EstClavierActiv�)
         {
            //p�dales + ajouter acc�l�ration??
            if (GestionInput.EstEnfonc�e(Keys.W) || GestionInput.EstEnfonc�e(Keys.S))
            {
               int sens = G�rerTouche(Keys.W) - G�rerTouche(Keys.S);
               Direction = 5* sens * Vector3.Normalize(new Vector3(-(float)Math.Cos(Rotation.Y), 0, (float)Math.Sin(Rotation.Y))) / 100f;
               Position += Direction;
               ChangementEffectu� = true;
            }
            //Volant... degr�s??
            if (GestionInput.EstEnfonc�e(Keys.A) || GestionInput.EstEnfonc�e(Keys.D))
            {
               int sens = G�rerTouche(Keys.A) - G�rerTouche(Keys.D);
               Rotation = new Vector3(Rotation.X, Rotation.Y + sens * INCR�MENT_ROTATION, Rotation.Z);
               ChangementEffectu� = true;
            }
            //D�placerCam�ra();
         }
      }
      int G�rerTouche(Keys Touche)
      {
         return GestionInput.EstEnfonc�e(Touche) ? 1 : 0;
      }
   }
}
