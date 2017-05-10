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

    abstract public class Chrono: Titre, IResettable
    {
       // protected float ValeurTimer { get; set; }

        public TimeSpan ValeurTimer { get; protected set; }
        float IntervalleDeMAJ { get; set; }
        float Temps�coul�DepuisMAJ { get; set; }
        public bool EstActif { get; set; }
        public Chrono(Game game,string format, TimeSpan d�part, string nomPolice, Vector2 position, string nomTexture, bool estActif, bool fond, Color couleur,float intervalleDeMAJ)
            : base(game, format, nomPolice, position, nomTexture, fond, couleur) // po legit
        {
            ValeurTimer = d�part;
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
                Incr�menter(gameTime.ElapsedGameTime);
            }
        }
        protected abstract void Incr�menter(TimeSpan val);
    }
}
