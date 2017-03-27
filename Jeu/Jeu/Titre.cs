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
    public class Titre : Microsoft.Xna.Framework.DrawableGameComponent
    {
        protected string Message { get; set; }
        protected SpriteBatch GestionSprites { get; private set; }
        string NomPolice { get; set; }
        SpriteFont Police { get; set; }
        Vector2 Position { get; set; }
        string NomTexture { get; set; }
        Texture2D Texture { get; set; }
        RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }
        Rectangle Destination { get; set; }
        public Titre(Game game, string message, string nomPolice, Vector2 position, string nomTexture)
        : base(game)
        {
            Message = message;
            Position = position;
            NomPolice = nomPolice;
            NomTexture = nomTexture;
        }
        protected override void LoadContent()
        {
            Police = Game.Content.Load<SpriteFont>("Fonts/" + NomPolice);
            Vector2 dimension = Police.MeasureString(Message);
            Position = new Vector2(Position.X - dimension.X / 2,
                                   Position.Y - dimension.Y / 2);
            GestionSprites = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
            GestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            Texture = GestionnaireDeTextures.Find(NomTexture);
            Destination = new Rectangle((int)Position.X, (int)Position.Y, (int)dimension.X, (int)dimension.Y);
            
        }
        public override void Draw(GameTime gameTime)
        {
            GestionSprites.Begin();
            GestionSprites.Draw(Texture, Destination, Color.White);
            GestionSprites.DrawString(Police, Message, Position, Color.Fuchsia);
            GestionSprites.End();
        }
    }
}
