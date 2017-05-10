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
    public class ChronoDiminue : Chrono
    {
        const string FORMAT = "ss";
        public ChronoDiminue(Game game, TimeSpan d�part, string nomPolice, Vector2 position, string nomTexture, bool estActif,bool fond, Color couleur, float intervalleDeMAJ)
            : base(game, (d�part).ToString(FORMAT), d�part, nomPolice, position, nomTexture, estActif, fond, couleur, intervalleDeMAJ)
        {
        }
        protected override void Incr�menter(TimeSpan val)
        {
            ValeurTimer -= val;
            if (ValeurTimer < new TimeSpan(0))
            {
                EstActif = false;
                ValeurTimer = new TimeSpan(0);
            }
            Message = ValeurTimer.ToString(FORMAT);
        }
    }
}
