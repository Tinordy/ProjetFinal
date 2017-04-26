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
    public class PisteSectionnée : PrimitiveDeBaseAnimée
    {
        DataPiste DonnéesPiste { get; set; }
        int HAUTEUR_INITIALE = 0;
        const float LARGEUR_PISTE = 7f;
        const float ÉCHELLE = 0.3f;

        Vector3 Origine { get; set; }
        protected List<List<Vector2>> PointsBordureExt { get; set; }
        protected List<List<Vector2>> PointsBordureInt { get; set; }
        protected List<List<Vector2>> PointsCentraux { get; set; }
        protected List<List<Vector2>> PointsPointillés { get; set; }
        public int NbPtsCentraux
        {
            get
            {
                return PointsCentraux.Count;
            }
        }
        Color CouleurPiste { get; set; }
        int NbDeTriangles { get; set; }
        int NbDeSommets { get; set; }
        List<VertexPositionColor[]> ListeSommets { get; set; }
        VertexPositionColor[] SommetsPointillés { get; set; }
        BasicEffect EffetDeBase { get; set; }
        int NbColonnes { get; set; }
        int NbRangées { get; set; }
        public BoundingSphere SphereDeCollision { get; private set; }
        Vector2 Coin { get; set; }
        Vector2 Étendue { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }
        public PisteSectionnée(Game jeu, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ, int nbColonnes, int nbRangées, Vector2 coin, Vector2 étendue)
            : base(jeu, homothétieInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            NbColonnes = nbColonnes;
            NbRangées = nbRangées;
            Coin = coin;
            Étendue = étendue;
        }
        public override void Initialize()
        {
            //Origine = new Vector3(-NbColonnes / 2, 25, -NbRangées / 2);
            //Origine = new Vector3(-25, 25, -25);
            Origine = Vector3.Zero;
            DonnéesPiste = Game.Services.GetService(typeof(DataPiste)) as DataPiste;
            CouleurPiste = Color.Black;
            ListeSommets = new List<VertexPositionColor[]>();
            ObtenirPointsCentraux();
            ObtenirDonnéesPiste();
            SphereDeCollision = new BoundingSphere(new Vector3(Origine.X + 25, 0, Origine.Z + 25), 75);

            //NbDeSommets = PointsBordureExt.Count + PointsBordureInt.Count + 2;

            base.Initialize();
        }
        protected override void InitialiserSommets()
        {
            if (NbPtsCentraux > 0)
            {

                for (int j = 0; j < PointsBordureExt.Count; ++j)
                {
                    NbSommets = PointsBordureExt[j].Count * 2 + 2;
                    VertexPositionColor[] sommets = new VertexPositionColor[NbSommets];
                    for (int i = 0; i < NbSommets - 3; i += 2)
                    {
                        float posXExt = Origine.X + PointsBordureExt[j][i / 2].X;
                        float posZExt = Origine.Z + PointsBordureExt[j][i / 2].Y;
                        float posXInt = Origine.X + PointsBordureInt[j][i / 2].X;
                        float posZInt = Origine.Z + PointsBordureInt[j][i / 2].Y;

                        sommets[i + 1] = new VertexPositionColor(new Vector3(posXExt, HAUTEUR_INITIALE, posZExt), CouleurPiste);
                        sommets[i] = new VertexPositionColor(new Vector3(posXInt, HAUTEUR_INITIALE, posZInt), CouleurPiste);
                    }
                    sommets[NbSommets - 2] = sommets[0];
                    sommets[NbSommets - 1] = sommets[1];
                    ListeSommets.Add(sommets);
                }
            }
            //InitialiserSommetsPointillés();

        }

        //protected void InitialiserSommetsPointillés()
        //{
        //    for (int i = 0; i < NbDeSommets - 3; i += 2)
        //    {
        //        SommetsPointillés[i + 1] = new VertexPositionColor(new Vector3(PointsPointillés[i + 1].X, HAUTEUR_INITIALE, PointsPointillés[i + 1].Y), Color.White);
        //        SommetsPointillés[i] = new VertexPositionColor(new Vector3(PointsPointillés[i].X, HAUTEUR_INITIALE, PointsPointillés[i].Y), Color.White);
        //    }
        //    SommetsPointillés[NbDeSommets - 2] = SommetsPointillés[0];
        //    SommetsPointillés[NbDeSommets - 1] = SommetsPointillés[1];
        //}

        protected override void LoadContent()
        {
            EffetDeBase = new BasicEffect(GraphicsDevice);
            InitialiserParamètresEffetDeBase();
            base.LoadContent();
        }
        public void ObtenirDonnéesPiste()
        {
            //PointsBordureExt = GénérerListeRestreinte(DonnéesPiste.GetBordureExtérieure());
            //PointsBordureInt = GénérerListeRestreinte(DonnéesPiste.GetBordureIntérieur());
            if (NbPtsCentraux > 0)
            {
                GénérerBordureÀPartirDeMilieu();
                //InitialiserSommets();
            }

            //PointsPointillés = GénérerListeRestreinte(DonnéesPiste.GetPointsPointillés());
        }

        void ObtenirPointsCentraux()
        {
            PointsCentraux = GénérerListeRestreinte(DonnéesPiste.GetPointsCentraux());
        }

        void GérerVisibilité()
        {
            if (CaméraJeu.Frustum.Intersects(SphereDeCollision))
            {
                //EnableDraw = true;
                Visible = true;
            }
            else
            {
                Visible = false;
                //EnableDraw = false;
            }
        }

        List<List<Vector2>> GénérerListeRestreinte(List<Vector2> points)
        {
            Vector2 margeDeManoeuvre = new Vector2(20, 20);
            Vector2 pointMax = Coin + Étendue + margeDeManoeuvre;
            int ancienIndex = -1;
            List<List<Vector2>> temp = new List<List<Vector2>>();
            List<Vector2> listePointTemp = new List<Vector2>();
            //foreach (Vector2 point in points)

            for (int i = 0; i < points.Count; ++i)
            {
                Vector2 point = points[i];
                if (point.X > Coin.X && point.X < pointMax.X && point.Y > Coin.Y && point.Y < pointMax.Y)
                {
                    if (ancienIndex != i - 1)
                    {
                        temp.Add(listePointTemp);
                        listePointTemp = new List<Vector2>();

                    }
                    listePointTemp.Add(point);

                    ancienIndex = i;
                }

            }
            temp.Add(listePointTemp);
            for (int i = 0; i < temp.Count; ++i)
            {
                if (temp[i].Count < 2)
                {
                    temp.RemoveAt(i);
                }
            }
            return temp;
        }

        void GénérerBordureÀPartirDeMilieu()
        {
            PointsBordureExt = new List<List<Vector2>>();
            PointsBordureInt = new List<List<Vector2>>();
            List<Vector2> tempInt = new List<Vector2>();
            List<Vector2> tempExt = new List<Vector2>();
            Vector2 vecteurPourBordure = new Vector2();
            foreach (List<Vector2> listePointsCentraux in PointsCentraux)
            {
                for (int i = 0; i < listePointsCentraux.Count; ++i)
                {
                    if (i < listePointsCentraux.Count - 1)
                    {
                        vecteurPourBordure = listePointsCentraux[i + 1] - listePointsCentraux[i];
                    }
                    else { vecteurPourBordure = listePointsCentraux[i] - listePointsCentraux[i - 1]; }

                    tempInt.Add(listePointsCentraux[i] + (LARGEUR_PISTE * (Vector2.Normalize(new Vector2(vecteurPourBordure.Y, -vecteurPourBordure.X)))));
                    tempExt.Add(listePointsCentraux[i] + (LARGEUR_PISTE * (Vector2.Normalize(new Vector2(-vecteurPourBordure.Y, vecteurPourBordure.X)))));
                }
                PointsBordureInt.Add(tempInt);
                tempInt = new List<Vector2>();
                PointsBordureExt.Add(tempExt);
                tempExt = new List<Vector2>();
            }
        }

        public override void Update(GameTime gameTime)
        {
            float TempsÉcoulé = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TempsÉcouléDepuisMAJ += TempsÉcoulé;
            if (TempsÉcouléDepuisMAJ >= IntervalleMAJ)
            {
                //GérerVisibilité();
                TempsÉcouléDepuisMAJ = 0;
            }
            base.Update(gameTime);
        }

        void InitialiserParamètresEffetDeBase()
        {
            EffetDeBase.VertexColorEnabled = true;
        }
        public override void Draw(GameTime gameTime)
        {
            DepthStencilState ancienDepthStencilState = GraphicsDevice.DepthStencilState;
            DepthStencilState temporaire = new DepthStencilState();
            temporaire.DepthBufferEnable = false;
            GraphicsDevice.DepthStencilState = temporaire;
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            EffetDeBase.World = GetMonde();
            EffetDeBase.View = CaméraJeu.Vue;
            EffetDeBase.Projection = CaméraJeu.Projection;

            foreach (VertexPositionColor[] Sommets in ListeSommets)
            {
                foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
                {
                    passeEffet.Apply();
                    NbDeTriangles = Sommets.Count() - 2;
                    if (NbDeTriangles > 0)
                    {
                        GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, Sommets, 0, NbDeTriangles);
                        //GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, SommetsPointillés, 0, NbDeTriangles);
                        //Problème à 60 / 0 / 135 -> Manque un bout de piste
                    }

                }
            }

            GraphicsDevice.DepthStencilState = ancienDepthStencilState;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }
        public List<Vector2> ObtenirVecteurPerp()
        {
            List<Vector2> vecteurs = new List<Vector2>();
            vecteurs.Add(PointsBordureExt[0][0] - PointsCentraux[0][0]);
            vecteurs.Add(PointsCentraux[0][0]);
            vecteurs.Add(PointsBordureInt[0][0] - PointsCentraux[0][0]);
            vecteurs.Add(PointsCentraux[0][0]);
            return vecteurs;
        }
        public List<Vector2> ObtenirVecteursPerp()
        {
            if (PointsCentraux.Count > 0)
            {

                List<Vector2> vecteurs = new List<Vector2>();
                for (int i = 0; i < PointsBordureExt.Count; ++i)
                {
                    vecteurs.Add(PointsBordureExt[0][i] - PointsCentraux[0][i]);
                    vecteurs.Add(PointsCentraux[0][i]);
                    vecteurs.Add(PointsBordureInt[0][i] - PointsCentraux[0][i]);
                    vecteurs.Add(PointsCentraux[0][i]);
                }
                return vecteurs;
            }
            return null;
        }
        public Vector2 ObtenirBordureExt(int i)
        {
            return PointsBordureExt[i][i*(PointsBordureExt[i].Count - 1)];
        }
        public Vector2 ObtenirBordureInt(int i)
        {
            return PointsBordureInt[i][i * (PointsBordureInt[i].Count - 1)];
        }
        public Vector2 ObtenirPremierPointCentral(int i)
        {
            return PointsCentraux[i][0];
        }
    }
}
