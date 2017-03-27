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
        float TempsÉcouléDepuisMAJ { get; set; }
        public bool EstActif { get; private set; }
        string Format { get; set; }
        public Timer(Game game,float départ, float fin, string nomPolice, Vector2 position, string nomTexture, bool estActif,string format, float intervalleDeMAJ)
            : base(game, départ.ToString(format), nomPolice, position, nomTexture)
        {
            ValeurTimer = départ;
            Augmente = (int)Math.Round((fin - départ) / Math.Abs((fin - départ)));
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
                float tempsÉcoulé = (float)gameTime.ElapsedGameTime.TotalSeconds;
                TempsÉcouléDepuisMAJ += tempsÉcoulé;
                if (TempsÉcouléDepuisMAJ >= IntervalleDeMAJ)
                {
                    ValeurTimer += Augmente * TempsÉcouléDepuisMAJ;
                    if ((ValeurTimer - EndValTimer) * Augmente >= 0)
                    {
                        EstActif = false;
                        ValeurTimer = EndValTimer;
                    }
                    Message = ValeurTimer.ToString(Format);
                    TempsÉcouléDepuisMAJ = 0;
                    //Recall bounds
                }
            }
        }
    }
}
