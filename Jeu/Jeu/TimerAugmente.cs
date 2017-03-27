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
    public class TimerAugmente : Timer
    { 
        const string FORMAT = "00:00.##"; //modulo 60
        public TimerAugmente(Game game, float d�part, string nomPolice, Vector2 position, string nomTexture, bool estActif, float intervalleDeMAJ)
            : base(game, (d�part + 0.01f).ToString(FORMAT), d�part, nomPolice, position, nomTexture, estActif, intervalleDeMAJ)
        {

        }
        protected override void Incr�menter(float val)
        {
            ValeurTimer += val;
            Message = ValeurTimer.ToString(FORMAT);
        }
    }
}
