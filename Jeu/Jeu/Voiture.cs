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
      const float INCRÉMENT_ROTATION = (float)Math.PI / 120;

      float IntervalleMAJ { get; set; }
      float TempsÉcouléDepuisMAJ { get; set; }
      InputManager GestionInput { get; set; }
      Vector3 Direction { get; set; }
      bool ChangementEffectué { get; set; }
      Caméra Caméra { get; set; }

      //Rotation... changement effextué? prp auto

      public Voiture(Game game, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ)
          : base(game, nomModèle, échelleInitiale, rotationInitiale, positionInitiale)
      {
         IntervalleMAJ = intervalleMAJ;
      }

      public override void Initialize()
      {
         //ok?
         Direction = new Vector3(-1, 0, 0);
         GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
         Caméra = Game.Services.GetService(typeof(Caméra)) as Caméra;
         base.Initialize();//keep
      }

      public override void Update(GameTime gameTime)
      {
         float tempsécoulé = (float)gameTime.ElapsedGameTime.TotalSeconds;
         TempsÉcouléDepuisMAJ += tempsécoulé;
         if (TempsÉcouléDepuisMAJ >= IntervalleMAJ)
         {
            GérerVolant();
            //DéplacerCaméra();
            EffectuerTransformations();
            TempsÉcouléDepuisMAJ = 0;
         }
      }
      //marche po...
      //private void DéplacerCaméra()
      //{
      //   Vector3 anciennePositionCaméra = Caméra.Position;
      //   Vector3 nouvellePositionCaméra = Position - Direction * 500 + new Vector3(0, 3, 0); //const hauteur de la caméra
      //   Vector3 déplacementCaméra = (nouvellePositionCaméra - anciennePositionCaméra) / 20;

      //   Caméra.Déplacer(anciennePositionCaméra + déplacementCaméra/*new Vector3(déplacementCaméra.X, 0, déplacementCaméra.Z)*/, Position, Vector3.Up);
      //}

      private void EffectuerTransformations()
      {
         if (ChangementEffectué)
         {
            Monde = Matrix.CreateScale(Échelle) * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) * Matrix.CreateTranslation(Position);
            ChangementEffectué = false;
         }
      }

      private void GérerVolant()
      {
         if (GestionInput.EstClavierActivé)
         {
            //pédales + ajouter accélération??
            if (GestionInput.EstEnfoncée(Keys.W) || GestionInput.EstEnfoncée(Keys.S))
            {
               int sens = GérerTouche(Keys.W) - GérerTouche(Keys.S);
               Direction = 5* sens * Vector3.Normalize(new Vector3(-(float)Math.Cos(Rotation.Y), 0, (float)Math.Sin(Rotation.Y))) / 100f;
               Position += Direction;
               ChangementEffectué = true;
            }
            //Volant... degrés??
            if (GestionInput.EstEnfoncée(Keys.A) || GestionInput.EstEnfoncée(Keys.D))
            {
               int sens = GérerTouche(Keys.A) - GérerTouche(Keys.D);
               Rotation = new Vector3(Rotation.X, Rotation.Y + sens * INCRÉMENT_ROTATION, Rotation.Z);
               ChangementEffectué = true;
            }
            //DéplacerCaméra();
         }
      }
      int GérerTouche(Keys Touche)
      {
         return GestionInput.EstEnfoncée(Touche) ? 1 : 0;
      }
   }
}
