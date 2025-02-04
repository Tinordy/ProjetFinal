﻿using System;
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
   //CONSTANTES!
   public class EntréeDeTexte : Microsoft.Xna.Framework.DrawableGameComponent, ISélectionnable
   {
      int NbCharMax { get; set; }
      Rectangle DestinationImage { get; set; }
      int NbChar { get; set; }
      Vector2 Position { get; set; }
      Vector2 OrigineChaîne { get; set; }
      string Entrée;
      SpriteBatch GestionSprite { get; set; }
      SpriteFont Font { get; set; }
      string Police { get; set; }
      Texture2D FondActif { get; set; }
      Texture2D FondInactif { get; set; }
      Texture2D Fond { get; set; }
      RessourcesManager<Texture2D> GestionnaireTexture2D { get; set; }
      RessourcesManager<SpriteFont> GestionnaireSpriteFont { get; set; }
      InputManager GestionInput { get; set; }
      Keys[] Touches { get; set; }
      char[] Caractères { get; set; }
      //bool EstActif { get; set; }
      bool estSélectionné;
      public bool EstSélectionné
      {
         get
         {
            return estSélectionné;
         }
         set
         {
            estSélectionné = value;
            if(estSélectionné)
            {
               Fond = FondActif; //mettre dans une liste, avec enum?
            }
            else
            {
               Fond = FondInactif;
            }
         }
      }
      public EntréeDeTexte(Game game, Vector2 position, string police, int nbCharMax)
          : base(game)
      {
         Position = position;
         Police = police;
         NbCharMax = nbCharMax;
      }
      public override void Initialize()
      {
         EstSélectionné = false;
         Entrée = "";

         GestionSprite = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
         GestionnaireSpriteFont = Game.Services.GetService(typeof(RessourcesManager<SpriteFont>)) as RessourcesManager<SpriteFont>;
         GestionnaireTexture2D = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
         Font = GestionnaireSpriteFont.Find(Police);
         FondActif = GestionnaireTexture2D.Find("BoutonVert");
         FondInactif = GestionnaireTexture2D.Find("BoutonNoir");
         Fond = FondInactif;
         //EstActif = false;
         GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
         NbChar = 0;
         CréerTableaux();
         Vector2 dimensionj = ObtenirDimensionRectangle();
         DestinationImage = new Rectangle((int)(Position.X - dimensionj.X / 2), (int)(Position.Y - dimensionj.Y / 2), (int)dimensionj.X, (int)dimensionj.Y);



         base.Initialize();
      }
      void CréerTableaux()
      {
         Caractères = new char[37] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' };
         Touches = new Keys[37] { Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z, Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.OemPeriod };
      }
      Vector2 ObtenirDimensionRectangle()
      {
         float GrandX = 0;
         float GrandY = 0;
         Vector2 f;
         //AUTRE?
         for (int i = 0; i < 36; i++)
         {
            f = Font.MeasureString(new string(Caractères[i], NbCharMax));
            if (f.X > GrandX)
            {
               GrandX = f.X;
            }
            if (f.Y > GrandY)
            {
               GrandY = f.Y;
            }
         }
         return 1.10f * new Vector2(GrandX, GrandY);
      }
      public override void Update(GameTime gameTime)
      {
         GérerSouris();
         if (/*EstActif*/EstSélectionné)
         {
            LireClavier();
         }

      }

      private void GérerSouris()
      {
         Point positionSouris = GestionInput.GetPositionSouris();
         if (GestionInput.EstNouveauClicGauche())
         {
            if (DestinationImage.Contains(positionSouris))
            {
               EstSélectionné/*EstActif*/ = true;
               //Fond = FondActif;
            }
            else
            {
               EstSélectionné = false;
               //Fond = FondInactif; //propriété?
            }
         }

      }

      private void LireClavier()
      {
         if (GestionInput.EstClavierActivé)
         {
            if (NbChar < NbCharMax)
            {
               for (int i = 0; i < 37; i++)
               {
                  if (GestionInput.EstNouvelleTouche(Touches[i]) && NbChar < NbCharMax) //do it again?
                  {
                     Entrée += Caractères[i];
                     ++NbChar;
                  }
               }
            }
            if (GestionInput.EstNouvelleTouche(Keys.Back) && NbChar > 0)
            {
               Entrée = Entrée.Remove(NbChar - 1, 1);
               NbChar--;
            }
            OrigineChaîne = Font.MeasureString(Entrée) / 2;
         }
      }

      public override void Draw(GameTime gameTime)
      {
         GestionSprite.Begin();
         GestionSprite.Draw(Fond, DestinationImage, Color.White);
         GestionSprite.DrawString(Font, Entrée, Position, Color.Black, 0, OrigineChaîne, 1f, SpriteEffects.None, 0);
         GestionSprite.End();
      }
      public string ObtenirEntrée()
      {
         return Entrée;
      }
   }
}
