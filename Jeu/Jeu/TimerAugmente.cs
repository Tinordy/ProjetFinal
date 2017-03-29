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
    public class TimerAugmente : Timer
    { 
        const string FORMAT = "mm':'ss','ff";
        public TimerAugmente(Game game, TimeSpan départ, string nomPolice, Vector2 position, string nomTexture, bool estActif, float intervalleDeMAJ)
            : base(game, (départ).ToString(FORMAT), départ, nomPolice, position, nomTexture, estActif, intervalleDeMAJ)
        {

        }
        protected override void Incrémenter(TimeSpan val)
        {
            ValeurTimer = ValeurTimer.Add(val);
            Message = ValeurTimer.ToString(FORMAT);
        }
    }
}
