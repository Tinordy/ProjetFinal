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

    public class VoitureDummy : Voiture, IResettable
    {
        const float RAYON_VOITURE_DUMMY = 0.2f;
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
        public VoitureDummy(Game game, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, int index, float intervalleMAJ)
            : base(game, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            Data = Game.Services.GetService(typeof(DataPiste)) as DataPiste;
            PointsCentraux = Data.GetPointsCentraux();
            Index = index;
        }
        public override void Initialize()
        {
            DistanceParcourue = 0;
            IndexIntermédiaire = 0;
            Position = new Vector3(PointsCentraux[Index].X, 0, PointsCentraux[Index].Y);
            base.Initialize();
        }
        public override void Update(GameTime gameTime)
        {
            TempsÉcouléDepuisMAJ += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (TempsÉcouléDepuisMAJ >= IntervalleMAJ)
            {
                DéplacerVoiture();
                RecréerMonde();
                TempsÉcouléDepuisMAJ = 0;
            }
        }
        Vector2 Déplacement { get; set; }
        float DistanceParcourue { get; set; }
        float DistanceÀParcourir { get; set;
        }
        private void DéplacerVoiture()
        {
            if (DistanceParcourue >= DistanceÀParcourir)
            {
                ++Index;
                Déplacement = PointsCentraux[(Index + 1) % PointsCentraux.Count] - PointsCentraux[Index];
                DistanceÀParcourir = Déplacement.Length();
                DistanceParcourue = 0;
                Déplacement = Vector2.Normalize(Déplacement)/10;
                Position = new Vector3(PointsCentraux[Index].X, 0, PointsCentraux[Index].Y);
                int signe = Déplacement.Y < 0 ? 0 : 1;
                Rotation = new Vector3(0,(float)(Math.Atan(Déplacement.X / Déplacement.Y) + signe*Math.PI),0);
                SphèreDeCollision = new BoundingSphere(Position, RAYON_VOITURE_DUMMY);
            }
            else
            {
                Position += new Vector3(Déplacement.X, 0, Déplacement.Y);
                SphèreDeCollision = new BoundingSphere(Position, RAYON_VOITURE_DUMMY);
                DistanceParcourue += Déplacement.Length();
            }

        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
