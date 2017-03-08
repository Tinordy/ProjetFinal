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
   public class EntréeDeTexte : Microsoft.Xna.Framework.DrawableGameComponent
   {
      const int NB_CHAR_MAX = 10;
      Rectangle DestinationImage { get; set; }
      int NbChar { get; set; }
      Vector2 Position { get; set; }
      Vector2 OrigineChaîne { get; set; }
      string Entrée;
      SpriteBatch GestionSprite { get; set; }
      SpriteFont Font { get; set; }
      string Police { get; set; }
      Texture2D Fond { get; set; }
      RessourcesManager<Texture2D> GestionnaireTexture2D { get; set; }
      RessourcesManager<SpriteFont> GestionnaireSpriteFont { get; set; }
      InputManager GestionInput { get; set; }
      Keys[] Touches { get; set; }
      char[] Caractères { get; set; }

      public EntréeDeTexte(Game game, Vector2 position, string police)
          : base(game)
      {
         Position = position;
         Police = police;
      }
      public override void Initialize()
      {
         Entrée = "";

         GestionSprite = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
         GestionnaireSpriteFont = Game.Services.GetService(typeof(RessourcesManager<SpriteFont>)) as RessourcesManager<SpriteFont>;
         GestionnaireTexture2D = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
         Font = GestionnaireSpriteFont.Find(Police);
         Fond = GestionnaireTexture2D.Find("BoutonVert");
         GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
         NbChar = 0;
         CréerTableaux();
         Vector2 dimensionj = ObtenirDimensionRectangle();
         DestinationImage = new Rectangle((int)(Position.X - dimensionj.X/2), (int)(Position.Y - dimensionj.Y/2), (int)dimensionj.X, (int)dimensionj.Y);



         base.Initialize();
      }
      void CréerTableaux()
      {
         Caractères = new char[36] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
         Touches = new Keys[36] { Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z, Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9 };
      }
      Vector2 ObtenirDimensionRectangle()
      {
         float GrandX = 0;
         float GrandY = 0;
         Vector2 f;
         //AUTRE?
         for (int i = 0; i < 36; i++)
         {
            f = Font.MeasureString(new string(Caractères[i], 10));
            if (f.X > GrandX)
            {
               GrandX = f.X;
            }
            if(f.Y > GrandY)
            {
               GrandY = f.Y;
            }
         }
         return 1.10f* new Vector2(GrandX, GrandY);
      }
      public override void Update(GameTime gameTime)
      {
         LireClavier();
         base.Update(gameTime);
      }

      private void LireClavier()
      {
         if(GestionInput.EstClavierActivé)
         {
            if(NbChar < NB_CHAR_MAX)
            {
               for (int i = 0; i < 36; i++)
               {
                  if (GestionInput.EstNouvelleTouche(Touches[i]) && NbChar < NB_CHAR_MAX) //do it again?
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
