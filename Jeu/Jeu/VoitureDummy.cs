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
        public VoitureDummy(Game game, string nomMod�le, float �chelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ)
            : base(game, nomMod�le, �chelleInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            
        }
        public override void Initialize()
        {
        
            IndexInterm�diaire = 0;
            Data = Game.Services.GetService(typeof(DataPiste)) as DataPiste;
            PointsCentraux = Data.GetPointsCentraux();
            Position = new Vector3(PointsCentraux[0].X, 0, PointsCentraux[0].Y);
            base.Initialize();
        }
        public override void Update(GameTime gameTime)
        {
            Temps�coul�DepuisMAJ += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(Temps�coul�DepuisMAJ >= IntervalleMAJ)
            {
                D�placerVoiture();
                Recr�erMonde();
            }
        }

        private void D�placerVoiture()
        {
            Vector2 temp = PointsCentraux[Index] - PointsCentraux[(Index + 1)%PointsCentraux.Count];
            Position = new Vector3(PointsCentraux[Index].X, 0,PointsCentraux[Index].Y);
            //Position += new Vector3(temp.X, 0, temp.Y)/10f;
            ++IndexInterm�diaire;
            if (IndexInterm�diaire == 10)
            {
                ++Index;
                IndexInterm�diaire = 0;
            }

        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
