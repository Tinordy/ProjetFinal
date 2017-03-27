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
    public class TimerDiminue : Timer
    {
        const string FORMAT = "0";
        public TimerDiminue(Game game, float départ, string nomPolice, Vector2 position, string nomTexture, bool estActif, float intervalleDeMAJ)
            : base(game, (départ + 0.01f).ToString(FORMAT), départ, nomPolice, position, nomTexture, estActif, intervalleDeMAJ)
        {
        }
        protected override void Incrémenter(float val)
        {
            ValeurTimer -= val;
            if (ValeurTimer < 0)
            {
                EstActif = false;
                ValeurTimer = 0;
            }
            Message = ValeurTimer.ToString(FORMAT);
        }
    }
}
