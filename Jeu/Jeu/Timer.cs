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
        float TempsÉcouléDepuisMAJ { get; set; }
        public bool EstActif { get; protected set; }
        public Timer(Game game,string format, float départ, string nomPolice, Vector2 position, string nomTexture, bool estActif, float intervalleDeMAJ)
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
            if(EstActif)
            {
                float tempsÉcoulé = (float)gameTime.ElapsedGameTime.TotalSeconds;
                TempsÉcouléDepuisMAJ += tempsÉcoulé;
                if (TempsÉcouléDepuisMAJ >= IntervalleDeMAJ)
                {
                    Incrémenter(TempsÉcouléDepuisMAJ);
                    TempsÉcouléDepuisMAJ = 0;
                }
            }
        }
        protected abstract void Incrémenter(float val);
    }
}
