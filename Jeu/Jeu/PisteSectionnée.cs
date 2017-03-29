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
        protected List<Vector2> PointsBordureExt { get; set; }
        protected List<Vector2> PointsBordureInt { get; set; }
        protected List<Vector2> PointsCentraux { get; set; }
        protected List<Vector2> PointsPointillés { get; set; }
        Color CouleurPiste { get; set; }
        int NbDeTriangles { get; set; }
        int NbDeSommets { get; set; }
        VertexPositionColor[] Sommets { get; set; }
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
            CouleurPiste = Color.Blue;
            ObtenirDonnéesPiste();
            NbDeSommets = PointsBordureExt.Count + PointsBordureInt.Count + 2;
            NbDeTriangles = NbDeSommets - 2;
            CréerTableauSommets();
            base.Initialize();
        }
        void CréerTableauSommets()
        {
            Sommets = new VertexPositionColor[NbDeSommets];
            SommetsPointillés = new VertexPositionColor[NbDeSommets];
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
            //InitialiserSommetsPointillés();
        }

        protected void InitialiserSommetsPointillés()
        {
            for (int i = 0; i < NbDeSommets - 3; i += 2)
            {
                SommetsPointillés[i + 1] = new VertexPositionColor(new Vector3(PointsPointillés[i + 1].X, HAUTEUR_INITIALE, PointsPointillés[i + 1].Y), Color.White);
                SommetsPointillés[i] = new VertexPositionColor(new Vector3(PointsPointillés[i].X, HAUTEUR_INITIALE, PointsPointillés[i].Y), Color.White);
            }
            SommetsPointillés[NbDeSommets - 2] = SommetsPointillés[0];
            SommetsPointillés[NbDeSommets - 1] = SommetsPointillés[1];
        }

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
            PointsCentraux = GénérerListeRestreinte(DonnéesPiste.GetPointsCentraux());
            PointsPointillés = GénérerListeRestreinte(DonnéesPiste.GetPointsPointillés());
        }

        List<Vector2> GénérerListeRestreinte(List<Vector2> points)
        {
            Vector2 pointMax = Coin + Étendue;
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
            if (NbDeTriangles != 0)
            {
                foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
                {
                    passeEffet.Apply();

                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, Sommets, 0, NbDeTriangles);
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, SommetsPointillés, 0, NbDeTriangles);

                }
            }
            GraphicsDevice.DepthStencilState = ancienDepthStencilState;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }
    }
}
