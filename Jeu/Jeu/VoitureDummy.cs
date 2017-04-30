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
        float Temps�coul�DepuisMAJ { get; set; }

        int IndexInterm�diaire { get; set; }
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
        public VoitureDummy(Game game, string nomMod�le, float �chelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, int index, float intervalleMAJ)
            : base(game, nomMod�le, �chelleInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            Data = Game.Services.GetService(typeof(DataPiste)) as DataPiste;
            PointsCentraux = Data.GetPointsCentraux();
            Index = index;
        }
        public override void Initialize()
        {
            DistanceParcourue = 0;
            IndexInterm�diaire = 0;
            Position = new Vector3(PointsCentraux[Index].X, 0, PointsCentraux[Index].Y);
            base.Initialize();
        }
        public override void Update(GameTime gameTime)
        {
            Temps�coul�DepuisMAJ += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Temps�coul�DepuisMAJ >= IntervalleMAJ)
            {
                D�placerVoiture();
                Recr�erMonde();
            }
        }
        Vector2 D�placement { get; set; }
        float DistanceParcourue { get; set; }
        float Distance�Parcourir { get; set;
        }
        private void D�placerVoiture()
        {
            if (DistanceParcourue >= Distance�Parcourir)
            {
                ++Index;
                D�placement = PointsCentraux[(Index + 1) % PointsCentraux.Count] - PointsCentraux[Index];
                Distance�Parcourir = D�placement.Length();
                DistanceParcourue = 0;
                D�placement = Vector2.Normalize(D�placement)/10;
                Position = new Vector3(PointsCentraux[Index].X, 0, PointsCentraux[Index].Y);
                int signe = D�placement.Y < 0 ? 0 : 1;
                Rotation = new Vector3(0,(float)(Math.Atan(D�placement.X / D�placement.Y) + signe*Math.PI),0);
                Sph�reDeCollision = new BoundingSphere(Position, RAYON_VOITURE_DUMMY);
            }
            else
            {
                Position += new Vector3(D�placement.X, 0, D�placement.Y);
                Sph�reDeCollision = new BoundingSphere(Position, RAYON_VOITURE_DUMMY);
                DistanceParcourue += D�placement.Length();
            }

        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
