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
        int HAUTEUR_INITIALE = 1;

        Vector3 Origine { get; set; }
        protected List<List<Vector2>> PointsBordureExt { get; set; }
        protected List<List<Vector2>> PointsBordureInt { get; set; }
        protected List<List<Vector2>> PointsCentraux { get; set; }
        protected List<List<Vector2>> PointsPointillés { get; set; }
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
            Origine = new Vector3(-NbColonnes / 2, 25, -NbRangées / 2);
            DonnéesPiste = Game.Services.GetService(typeof(DataPiste)) as DataPiste;
            CouleurPiste = Color.Black;
            ObtenirPointsCentraux();
            ObtenirDonnéesPiste();
            //NbDeSommets = PointsBordureExt.Count + PointsBordureInt.Count + 2;
            NbDeTriangles = NbDeSommets - 2;
            CréerTableauSommets();
            InitialiserSommets();
            base.Initialize();
        }
        void CréerTableauSommets()
        {
            ListeSommets = new List<VertexPositionColor[]>();
            //SommetsPointillés = new VertexPositionColor[NbDeSommets];
        }
        protected override void InitialiserSommets()
        {

            for (int j = 0; j < Math.Min(PointsBordureExt.Count, PointsBordureInt.Count); ++j)
            {
                NbSommets = PointsBordureExt[j].Count + PointsBordureInt[j].Count + 2;
                VertexPositionColor[] sommets = new VertexPositionColor[NbSommets];
                for (int i = 0; i < Math.Min(PointsBordureExt[j].Count, PointsBordureInt[j].Count) * 2 - 3; i += 2)
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
                //InitialiserSommetsPointillés();
            }
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
        void ObtenirDonnéesPiste()
        {
            PointsBordureExt = GénérerListeRestreinte(DonnéesPiste.GetBordureExtérieure());
            PointsBordureInt = GénérerListeRestreinte(DonnéesPiste.GetBordureIntérieur());

            PointsPointillés = GénérerListeRestreinte(DonnéesPiste.GetPointsPointillés());
        }

        void ObtenirPointsCentraux()
        {
            PointsCentraux = GénérerListeRestreinte(DonnéesPiste.GetPointsCentraux());
        }

        List<List<Vector2>> GénérerListeRestreinte(List<Vector2> points)
        {
            Vector2 pointMax = Coin + Étendue;
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
            return temp;
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
                    }

                }
            }

            GraphicsDevice.DepthStencilState = ancienDepthStencilState;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }
    }
}
