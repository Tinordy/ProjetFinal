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

    abstract public class Timer: Titre
    {
       // protected float ValeurTimer { get; set; }

        protected TimeSpan ValeurTimer { get; set; }
        float IntervalleDeMAJ { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }
        public bool EstActif { get; set; }
        public Timer(Game game,string format, TimeSpan départ, string nomPolice, Vector2 position, string nomTexture, bool estActif, float intervalleDeMAJ)
            : base(game, format, nomPolice, position, nomTexture) // po legit
        {
            ValeurTimer = départ;
            EstActif = estActif;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (EstActif)
            {
                Incrémenter(gameTime.ElapsedGameTime);
            }
        }
        protected abstract void Incrémenter(TimeSpan val);
    }
}
