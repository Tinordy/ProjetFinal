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

    public class VoitureDummy : Voiture
    {
        DataPiste Data { get; set; }
        List<Vector2> PointsCentraux { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }
        public VoitureDummy(Game game, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ)
            : base(game, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            Data = Game.Services.GetService(typeof(DataPiste)) as DataPiste;
            PointsCentraux = Data.GetPointsCentraux();
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            TempsÉcouléDepuisMAJ += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(TempsÉcouléDepuisMAJ >= IntervalleMAJ)
            {
                DéplacerVoiture();
                RecréerMonde();
            }
        }

        private void DéplacerVoiture()
        {
            Vector2 tamp = Vector2.Normalize(PointsCentraux[1] - PointsCentraux[2]);
            Position += new Vector3(tamp.X, 0, tamp.Y)/100f;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
