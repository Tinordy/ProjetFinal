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
        const float RAYON_VOITURE_DUMMY = 0.5f;
        DataPiste Data { get; set; }
        List<Vector2> PointsCentraux { get; set; }
        float Temps…coulÈDepuisMAJ { get; set; }

        int IndexIntermÈdiaire { get; set; }
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
        public VoitureDummy(Game game, string nomModËle, float ÈchelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, int index, float intervalleMAJ)
            : base(game, nomModËle, ÈchelleInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            Data = Game.Services.GetService(typeof(DataPiste)) as DataPiste;
            PointsCentraux = Data.GetPointsCentraux();
            Index = index;
        }
        public override void Initialize()
        {
            DistanceParcourue = 0;
            IndexIntermÈdiaire = 0;
            Position = new Vector3(PointsCentraux[Index].X, 0, PointsCentraux[Index].Y);
            base.Initialize();
        }
        public override void Update(GameTime gameTime)
        {
            Temps…coulÈDepuisMAJ += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Temps…coulÈDepuisMAJ >= IntervalleMAJ)
            {
                DÈplacerVoiture();
                RecrÈerMonde();
            }
        }
        Vector2 DÈplacement { get; set; }
        float DistanceParcourue { get; set; }
        float Distance¿Parcourir { get; set;
        }
        private void DÈplacerVoiture()
        {
            if (DistanceParcourue >= Distance¿Parcourir)
            {
                ++Index;
                DÈplacement = PointsCentraux[(Index + 1) % PointsCentraux.Count] - PointsCentraux[Index];
                Distance¿Parcourir = DÈplacement.Length();
                DistanceParcourue = 0;
                DÈplacement = Vector2.Normalize(DÈplacement)/10;
                Position = new Vector3(PointsCentraux[Index].X, 0, PointsCentraux[Index].Y);
                int signe = DÈplacement.Y < 0 ? 0 : 1;
                Rotation = new Vector3(0,(float)(Math.Atan(DÈplacement.X / DÈplacement.Y) + signe*Math.PI),0);
                SphËreDeCollision = new BoundingSphere(Position, RAYON_VOITURE_DUMMY);
            }
            else
            {
                Position += new Vector3(DÈplacement.X, 0, DÈplacement.Y);
                SphËreDeCollision = new BoundingSphere(Position, RAYON_VOITURE_DUMMY);
                DistanceParcourue += DÈplacement.Length();
            }

        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
