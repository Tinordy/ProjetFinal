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

        int IndexIntermédiaire { get; set; }
        int index;
        int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value % PointsCentraux.Count;
            }
        }
        public VoitureDummy(Game game, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ)
            : base(game, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            
        }
        public override void Initialize()
        {
        
            IndexIntermédiaire = 0;
            Data = Game.Services.GetService(typeof(DataPiste)) as DataPiste;
            PointsCentraux = Data.GetPointsCentraux();
            Position = new Vector3(PointsCentraux[0].X, 0, PointsCentraux[0].Y);
            base.Initialize();
        }
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
            Vector2 temp = PointsCentraux[Index] - PointsCentraux[(Index + 1)%PointsCentraux.Count];
            Position = new Vector3(PointsCentraux[Index].X, 0,PointsCentraux[Index].Y);
            //Position += new Vector3(temp.X, 0, temp.Y)/10f;
            ++IndexIntermédiaire;
            if (IndexIntermédiaire == 10)
            {
                ++Index;
                IndexIntermédiaire = 0;
            }

        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
