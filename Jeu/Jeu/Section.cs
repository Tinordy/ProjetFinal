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
    public class Section : Terrain
    {
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;
        Vector2 �tendue { get; set; }
        Vector2 Extr�mit� { get; set; }
        double Norme�tendue { get; set; }
        public Maison Maison { get; private set; }
        PisteSectionn�e Piste { get; set; }

        List<DrawableGameComponent> Components { get; set; }
        public Section(Game game, Vector2 origine, Vector2 �tendue2, float homoth�tieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, Vector3 �tendue,
                       string[] nomsTexturesTerrain, float intervalleMAJ)
            : base(game, origine, homoth�tieInitiale, rotationInitiale, positionInitiale, �tendue, nomsTexturesTerrain, intervalleMAJ)
        {
            //�tendue = �tendue2;
            �tendue = new Vector2(�tendue.X, �tendue.Z);

        }

        public override void Initialize()
        {
            Components = new List<DrawableGameComponent>();
            Extr�mit� = Coin + �tendue;
            Norme�tendue = Math.Sqrt(Math.Pow(�tendue.X, 2) + Math.Pow(�tendue.Y, 2));
            Cr�erPiste();
            Cr�erMaisons();
            base.Initialize();
        }

        private void Cr�erMaisons()
        {
            if (Piste.NbPtsCentraux > 0)
            {
                Vector2 temp = 4*Piste.ObtenirVecteurPerp()[0] + Piste.ObtenirVecteurPerp()[1];
                Maison = new Maison(Game, 5f, Vector3.Zero, new Vector3(temp.X,0,temp.Y), new Vector3(2, 2, 2), "brique1", "roof", 0.01f);
                Components.Add(Maison);
                Game.Components.Add(Maison);
            }

        }
        private void Cr�erPiste()
        {

            Piste = new PisteSectionn�e(Game,1 , Vector3.Zero, Vector3.Zero, INTERVALLE_MAJ_STANDARD, 20000, 20000, Coin, �tendue);
            Piste.Initialize();
        
            Components.Add(Piste);
            Game.Components.Add(Piste);
        }
        protected override void OnVisibleChanged(object sender, EventArgs args)
        {
            foreach (DrawableGameComponent c in Components)
            {
                c.Visible = Visible;
            }
        }

        //public void AddComponent(GameComponent x)
        //{
        //    Components.Add(x);
        //}
    }
}
