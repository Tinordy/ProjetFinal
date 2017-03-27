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
        protected float ValeurTimer { get; set; }
        float IntervalleDeMAJ { get; set; }
        float Temps�coul�DepuisMAJ { get; set; }
        public bool EstActif { get; protected set; }
        public Timer(Game game,string format, float d�part, string nomPolice, Vector2 position, string nomTexture, bool estActif, float intervalleDeMAJ)
            : base(game, format, nomPolice, position, nomTexture) // po legit
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
            if(EstActif)
            {
                float temps�coul� = (float)gameTime.ElapsedGameTime.TotalSeconds;
                Temps�coul�DepuisMAJ += temps�coul�;
                if (Temps�coul�DepuisMAJ >= IntervalleDeMAJ)
                {
                    Incr�menter(Temps�coul�DepuisMAJ);
                    Temps�coul�DepuisMAJ = 0;
                }
            }
        }
        protected abstract void Incr�menter(float val);
    }
}
