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

    public class Timer: Titre
    {
        float ValeurTimer { get; set; }
        float EndValTimer { get; set; }
        int Augmente { get; set; }
        float IntervalleDeMAJ { get; set; }
        float Temps…coulÈDepuisMAJ { get; set; }
        public bool EstActif { get; private set; }
        string Format { get; set; }
        public Timer(Game game,float dÈpart, float fin, string nomPolice, Vector2 position, string nomTexture, bool estActif,string format, float intervalleDeMAJ)
            : base(game, dÈpart.ToString(format), nomPolice, position, nomTexture)
        {
            ValeurTimer = dÈpart;
            Augmente = (int)Math.Round((fin - dÈpart) / Math.Abs((fin - dÈpart)));
            EstActif = estActif;
            EndValTimer = fin;
            Format = format;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if(EstActif)
            {
                float temps…coulÈ = (float)gameTime.ElapsedGameTime.TotalSeconds;
                Temps…coulÈDepuisMAJ += temps…coulÈ;
                if (Temps…coulÈDepuisMAJ >= IntervalleDeMAJ)
                {
                    ValeurTimer += Augmente * Temps…coulÈDepuisMAJ;
                    if ((ValeurTimer - EndValTimer) * Augmente >= 0)
                    {
                        EstActif = false;
                        ValeurTimer = EndValTimer;
                    }
                    Message = ValeurTimer.ToString(Format);
                    Temps…coulÈDepuisMAJ = 0;
                    //Recall bounds
                }
            }
        }
    }
}
