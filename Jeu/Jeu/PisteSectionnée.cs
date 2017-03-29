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
        protected List<Vector2> PointsBordureExt { get; set; }
        protected List<Vector2> PointsBordureInt { get; set; }
        protected List<Vector2> PointsCentraux { get; set; }
        protected List<Vector2> PointsPointill�s { get; set; }
        Color CouleurPiste { get; set; }
        int NbDeTriangles { get; set; }
        int NbDeSommets { get; set; }
        VertexPositionColor[] Sommets { get; set; }
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
            CouleurPiste = Color.Blue;
            ObtenirDonn�esPiste();
            NbDeSommets = PointsBordureExt.Count + PointsBordureInt.Count + 2;
            NbDeTriangles = NbDeSommets - 2;
            Cr�erTableauSommets();
            base.Initialize();
        }
        void Cr�erTableauSommets()
        {
            Sommets = new VertexPositionColor[NbDeSommets];
            SommetsPointill�s = new VertexPositionColor[NbDeSommets];
        }
        protected override void InitialiserSommets()
        {
            for (int i = 0; i < Math.Min(PointsBordureExt.Count, PointsBordureInt.Count) * 2 - 3; i += 2)
            {
                float posXExt = Origine.X + PointsBordureExt[i / 2].X;
                float posZExt = Origine.Z + PointsBordureExt[i / 2].Y;
                float posXInt = Origine.X + PointsBordureInt[i / 2].X;
                float posZInt = Origine.Z + PointsBordureInt[i / 2].Y;

                Sommets[i + 1] = new VertexPositionColor(new Vector3(posXExt, HAUTEUR_INITIALE, posZExt), CouleurPiste);
                Sommets[i] = new VertexPositionColor(new Vector3(posXInt, HAUTEUR_INITIALE, posZInt), CouleurPiste);
            }
            Sommets[NbDeSommets - 2] = Sommets[0];
            Sommets[NbDeSommets - 1] = Sommets[1];
            //InitialiserSommetsPointill�s();
        }

        protected void InitialiserSommetsPointill�s()
        {
            for (int i = 0; i < NbDeSommets - 3; i += 2)
            {
                SommetsPointill�s[i + 1] = new VertexPositionColor(new Vector3(PointsPointill�s[i + 1].X, HAUTEUR_INITIALE, PointsPointill�s[i + 1].Y), Color.White);
                SommetsPointill�s[i] = new VertexPositionColor(new Vector3(PointsPointill�s[i].X, HAUTEUR_INITIALE, PointsPointill�s[i].Y), Color.White);
            }
            SommetsPointill�s[NbDeSommets - 2] = SommetsPointill�s[0];
            SommetsPointill�s[NbDeSommets - 1] = SommetsPointill�s[1];
        }

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
            PointsCentraux = G�n�rerListeRestreinte(Donn�esPiste.GetPointsCentraux());
            PointsPointill�s = G�n�rerListeRestreinte(Donn�esPiste.GetPointsPointill�s());
        }

        List<Vector2> G�n�rerListeRestreinte(List<Vector2> points)
        {
            Vector2 pointMax = Coin + �tendue;
            List<Vector2> temp = new List<Vector2>();
            foreach (Vector2 point in points)
            {
                if (point.X > Coin.X && point.X < pointMax.X && point.Y > Coin.Y && point.Y < pointMax.Y)
                {
                    temp.Add(point);
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
            if (NbDeTriangles != 0)
            {
                foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
                {
                    passeEffet.Apply();

                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, Sommets, 0, NbDeTriangles);
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, SommetsPointill�s, 0, NbDeTriangles);

                }
            }
            GraphicsDevice.DepthStencilState = ancienDepthStencilState;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }
    }
}
