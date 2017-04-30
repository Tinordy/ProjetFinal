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
        const float HOMOTH�TIE_MAISON = 10f;
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;
        Vector2 �tendue { get; set; }
        Vector2 Extr�mit� { get; set; }
        double Norme�tendue { get; set; }
        PisteSectionn�e Piste { get; set; }
        //public Banni�reArriv�e Banni�re { get; private set; }
        List<DrawableGameComponent> Components { get; set; }

        bool ContientMaison { get; set; }
        public Section(Game game, Vector2 origine, Vector2 �tendue2, float homoth�tieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, Vector3 �tendue,
                       string[] nomsTexturesTerrain,bool containsHouse, float intervalleMAJ)
            : base(game, origine, homoth�tieInitiale, rotationInitiale, positionInitiale, �tendue, nomsTexturesTerrain, intervalleMAJ)
        {
            //�tendue = �tendue2;
            �tendue = new Vector2(�tendue.X, �tendue.Z);
            ContientMaison = containsHouse;
        }

        public override void Initialize()
        {
            Components = new List<DrawableGameComponent>();
            Extr�mit� = Coin + �tendue;
            Norme�tendue = Math.Sqrt(Math.Pow(�tendue.X, 2) + Math.Pow(�tendue.Y, 2));
            Cr�erPiste();
            Cr�erMaisons();
            base.Initialize();
            Visible = false;
        }

        private void Cr�erMaisons()
        {
            if (ContientMaison && Piste.NbPtsCentraux > 0)
            {
                List<Vector2> vecteursPerpendiculaires = Piste.ObtenirVecteurPerp();
                for(int i = 0; i < vecteursPerpendiculaires.Count; i += 2)
                {
                    Vector2 temp = 4 * vecteursPerpendiculaires[i] + vecteursPerpendiculaires[i+1];
                    Maison maison = new Maison(Game,HOMOTH�TIE_MAISON, Vector3.Zero, new Vector3(temp.X, 0, temp.Y), new Vector3(2, 2, 2), "brique1", "roof", 0.01f);
                    Components.Add(maison);
                    Game.Components.Add(maison);
                }
            }

        }
        private void Cr�erPiste()
        {

            Piste = new PisteSectionn�e(Game,1 , Vector3.Zero, Vector3.Zero, INTERVALLE_MAJ_STANDARD, 20000, 20000, Coin, �tendue);
            Piste.Initialize();
        
            Components.Add(Piste);
            Game.Components.Add(Piste);
            CreateLimits();
        }
        void CreateLimits()
        {
            List<Vector2> VecteursPerp = Piste.ObtenirVecteursPerp((int)(Origine.X % �tendue.X) + 7 * (int)(Origine.Y % �tendue.Y));
            if (VecteursPerp != null)
            {


                for (int i = 0; i < VecteursPerp.Count / 2; i += 4)
                {
                    Vector2 vUp = 4f * VecteursPerp[i] + VecteursPerp[i + 1];
                    Vector2 vDown = 2f * VecteursPerp[i + 2] + VecteursPerp[i + 3];
                    CubeColor� cubeUp = new CubeColor�(Game, 1f, Vector3.Zero, new Vector3(vUp.X,0,vUp.Y), Color.Yellow,new Vector3(2,2,2), INTERVALLE_MAJ_STANDARD);
                    CubeColor� cubeDown = new CubeColor�(Game, 1f, Vector3.Zero, new Vector3(vDown.X, 0, vDown.Y), Color.Gray, new Vector3(2, 2, 2), INTERVALLE_MAJ_STANDARD);
                    Components.Add(cubeUp);
                    Components.Add(cubeDown);
                    Game.Components.Add(cubeUp);
                    Game.Components.Add(cubeDown);
                }
            }
            
        }
        protected override void OnVisibleChanged(object sender, EventArgs args)
        {
            foreach (DrawableGameComponent c in Components)
            {
                c.Visible = Visible;
            }
        }
        public bool EstEnCollisionAvecUnObjet(ICollisionable objet)
        {
            bool valRetour = false;
            int i = 0;
            while(!valRetour && i < Components.Count)
            {
                valRetour = objet.EstEnCollision(Components[i]);
                ++i;
            }
            return valRetour;
        }
        //public PisteSectionn�e ObtenirSectionPiste()
        //{
        //    return Piste;
        //}
        public Banni�reArriv�e Cr�erBanni�re()
        {
            Vector2[] pointsA = new Vector2[4];

            pointsA[2] = Piste.ObtenirBordureExt(0);
            pointsA[0] = Piste.ObtenirBordureExt(1);
            pointsA[3] = Piste.ObtenirBordureInt(0);
            pointsA[1] = Piste.ObtenirBordureInt(1);

            Vector3[] points = new Vector3[4];
            for (int i = 0; i < 4; ++i)
            {
                points[i] = new Vector3(pointsA[i].X, 0, pointsA[i].Y);
            }

            Banni�reArriv�e Banni�re = new Banni�reArriv�e(Game, 1f, Vector3.Zero, Vector3.Zero, points, 0.01f);
            Components.Add(Banni�re);
            Game.Components.Add(Banni�re);
            return Banni�re;
        }
        public Vector2 ObtenirPointD�part()
        {
            return Piste.ObtenirPremierPointCentral(1);
        }

        public BoundingSphere Cr�erCheckPoint()
        {
            Vector2 v = Piste.ObtenirPremierPointCentral(0);
            return new BoundingSphere(new Vector3(v.X, 0, v.Y), 10);
        }
        //public void AddComponent(GameComponent x)
        //{
        //    Components.Add(x);
        //}
    }
}
