using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace AtelierXNA
{
   public class Sprite : Microsoft.Xna.Framework.DrawableGameComponent
   {
      string NomImage { get; set; }
      //protected Vector2 Position { get; set; }
      protected Texture2D Image { get; private set; } 
      SpriteBatch GestionSprites { get; set; }
      Rectangle RectangleSource { get; set; }

      public Sprite(Game jeu, string nomImage, Rectangle rectangleSource)
         : base(jeu)
      {
         NomImage = nomImage;
         RectangleSource = rectangleSource;
      }

      protected override void LoadContent()
      {
         GestionSprites = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
         Image = Game.Content.Load<Texture2D>("Textures/" + NomImage);
      }

      public override void Draw(GameTime gameTime)
      {
         GestionSprites.Begin();
         GestionSprites.Draw(Image, RectangleSource, Color.White);
         GestionSprites.End();
         //base.Draw(gameTime);
      }
   }
}