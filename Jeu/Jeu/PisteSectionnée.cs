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
    public class PisteSectionn�e : PrimitiveDeBaseAnim�e
    {
        DataPiste Donn�esPiste { get; set; }
        int HAUTEUR_INITIALE = 1;

        Vector3 Origine { get; set; }
        protected List<List<Vector2>> PointsBordureExt { get; set; }
        protected List<List<Vector2>> PointsBordureInt { get; set; }
        protected List<List<Vector2>> PointsCentraux { get; set; }
        protected List<List<Vector2>> PointsPointill�s { get; set; }
        Color CouleurPiste { get; set; }
        int NbDeTriangles { get; set; }
        int NbDeSommets { get; set; }
        List<VertexPositionColor[]> ListeSommets { get; set; }
        VertexPositionColor[] SommetsPointill�s { get; set; }
        BasicEffect EffetDeBase { get; set; }
        int NbColonnes { get; set; }
        int NbRang�es { get; set; }
        public BoundingSphere SphereDeCollision { get; private set; }
        Vector2 Coin { get; set; }
        Vector2 �tendue { get; set; }
        public PisteSectionn�e(Game jeu, float homoth�tieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ, int nbColonnes, int nbRang�es, Vector2 coin, Vector2 �tendue)
            : base(jeu, homoth�tieInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            NbColonnes = nbColonnes;
            NbRang�es = nbRang�es;
            Coin = coin;
            �tendue = �tendue;
        }
        public override void Initialize()
        {
            Origine = new Vector3(-NbColonnes / 2, 25, -NbRang�es / 2);
            Donn�esPiste = Game.Services.GetService(typeof(DataPiste)) as DataPiste;
            CouleurPiste = Color.Black;
            ObtenirPointsCentraux();
            ObtenirDonn�esPiste();
            //NbDeSommets = PointsBordureExt.Count + PointsBordureInt.Count + 2;
            NbDeTriangles = NbDeSommets - 2;
            Cr�erTableauSommets();
            InitialiserSommets();
            base.Initialize();
        }
        void Cr�erTableauSommets()
        {
            ListeSommets = new List<VertexPositionColor[]>();
            //SommetsPointill�s = new VertexPositionColor[NbDeSommets];
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
                //InitialiserSommetsPointill�s();
            }
        }

        //protected void InitialiserSommetsPointill�s()
        //{
        //    for (int i = 0; i < NbDeSommets - 3; i += 2)
        //    {
        //        SommetsPointill�s[i + 1] = new VertexPositionColor(new Vector3(PointsPointill�s[i + 1].X, HAUTEUR_INITIALE, PointsPointill�s[i + 1].Y), Color.White);
        //        SommetsPointill�s[i] = new VertexPositionColor(new Vector3(PointsPointill�s[i].X, HAUTEUR_INITIALE, PointsPointill�s[i].Y), Color.White);
        //    }
        //    SommetsPointill�s[NbDeSommets - 2] = SommetsPointill�s[0];
        //    SommetsPointill�s[NbDeSommets - 1] = SommetsPointill�s[1];
        //}

        protected override void LoadContent()
        {
            EffetDeBase = new BasicEffect(GraphicsDevice);
            InitialiserParam�tresEffetDeBase();
            base.LoadContent();
        }
        void ObtenirDonn�esPiste()
        {
            PointsBordureExt = G�n�rerListeRestreinte(Donn�esPiste.GetBordureExt�rieure());
            PointsBordureInt = G�n�rerListeRestreinte(Donn�esPiste.GetBordureInt�rieur());

            PointsPointill�s = G�n�rerListeRestreinte(Donn�esPiste.GetPointsPointill�s());
        }

        void ObtenirPointsCentraux()
        {
            PointsCentraux = G�n�rerListeRestreinte(Donn�esPiste.GetPointsCentraux());
        }

        List<List<Vector2>> G�n�rerListeRestreinte(List<Vector2> points)
        {
            Vector2 pointMax = Coin + �tendue;
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

        void InitialiserParam�tresEffetDeBase()
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
            EffetDeBase.View = Cam�raJeu.Vue;
            EffetDeBase.Projection = Cam�raJeu.Projection;

            foreach (VertexPositionColor[] Sommets in ListeSommets)
            {
                foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
                {
                    passeEffet.Apply();
                    NbDeTriangles = Sommets.Count() - 2;
                    if (NbDeTriangles > 0)
                    {
                        GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, Sommets, 0, NbDeTriangles);
                        //GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, SommetsPointill�s, 0, NbDeTriangles);
                    }

                }
            }

            GraphicsDevice.DepthStencilState = ancienDepthStencilState;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }
    }
}
