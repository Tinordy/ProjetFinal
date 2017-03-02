using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AtelierXNA
{//fini, sauf les normales
    public class Terrain : PrimitiveDeBaseAnimée
    {
        const int NB_TRIANGLES_PAR_TUILE = 2;
        const int NB_SOMMETS_PAR_TRIANGLE = 3;
        const int NB_COLONNES_RANGÉES = 20; // jouer avec les fps 

        Vector3 Étendue { get; set; }

        string NomCarteTerrain { get; set; }
        string[] NomsTexturesTerrain { get; set; }
        BasicEffect EffetDeBase { get; set; }
        RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }
        Texture2D TextureHerbe { get; set; }
        Texture2D TextureSable { get; set; }
        Texture2D TextureFusion { get; set; }
        Texture2D TextureBase { get; set; }
        Vector3 Origine { get; set; }
        public int NbColonnes { get; private set; }
        public int NbRangées { get; private set; }
        float DeltaX { get; set; }
        float DeltaY { get; set; }
        float DeltaZ { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }
        Vector2[,] PointsTexture { get; set; }
        Vector3[,] Points { get; set; }
        Vector3[,] Normales { get; set; }
        int NbTrianglesDansTerrain { get; set; }
        VertexPositionTexture[] Sommets { get; set; }
        float DeltaTextureX { get; set; }
        float DeltaTextureY { get; set; }
        Vector3 PositionCaméra { get; set; }
        Vector3 CibleCaméra { get; set; }
        protected Vector2 Coin { get; private set; }
        Point[] Sections { get; set; }
        public BoundingSphere SphereDeCollision { get; private set; }
        bool EnableDraw { get; set; }

        // à compléter en ajoutant les propriétés qui vous seront nécessaires pour l'implémentation du composant


        public Terrain(Game jeu, Vector2 coin, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, Vector3 étendue,
                       string[] nomsTexturesTerrain, float intervalleMAJ)

           : base(jeu, homothétieInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            Coin = coin;
            Étendue = étendue;
            NomsTexturesTerrain = nomsTexturesTerrain;

        }

        public override void Initialize()
        {
            GestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            base.Initialize();
            SphereDeCollision = new BoundingSphere(new Vector3(Coin.X + DeltaX, 0, Coin.Y - DeltaZ), 200);
        }

        //
        // à partir de la texture servant de carte de hauteur (HeightMap), on initialise les données
        // relatives à la structure de la carte
        //
        void InitialiserDonnéesCarte()
        {
            NbColonnes = NB_COLONNES_RANGÉES;
            NbRangées = NB_COLONNES_RANGÉES;
            DeltaZ = Étendue.Z / NbRangées;
            DeltaX = Étendue.X / NbColonnes;
            DeltaY = Étendue.Y;
        }

        //
        // Allocation des deux tableaux
        //    1) celui contenant les points de sommet (les points uniques), 
        //    2) celui contenant les sommets servant à dessiner les triangles
        void AllouerTableaux()
        {
            InitialiserDonnéesCarte();
            NbTrianglesDansTerrain = NbColonnes * NB_TRIANGLES_PAR_TUILE * NbRangées;
            Origine = new Vector3(Coin.X, 0, Coin.Y);
            DeltaTextureX = (float)1 / NbColonnes;
            DeltaTextureY = (float)1 / NbRangées;
            CréerTableauPoints();
            CréerTableauPointsTexture();

        }

        protected override void LoadContent()
        {
            base.LoadContent();

            EffetDeBase = new BasicEffect(GraphicsDevice);
            TextureHerbe = GestionnaireDeTextures.Find(NomsTexturesTerrain[0]);
            TextureSable = GestionnaireDeTextures.Find(NomsTexturesTerrain[1]);
            AllouerTableaux();
            InitialiserParamètresEffetDeBase();
            InitialiserSommets();
        }

        void InitialiserParamètresEffetDeBase()
        {
            EffetDeBase.TextureEnabled = true;
            EffetDeBase.Texture = TextureHerbe;
        }

        //
        // Création du tableau des points de sommets (on crée les points)
        // Ce processus implique la transformation des points 2D de la texture en coordonnées 3D du terrain
        //
        private void CréerTableauPoints()
        {
            int noSommet = 0;
            Points = new Vector3[NbColonnes + 1, NbRangées + 1];
            for (int row = 0; row < NbRangées + 1; ++row)
            {
                for (int col = 0; col < NbColonnes + 1; ++col)
                {
                    Points[NbColonnes - col, row] = new Vector3(Origine.X + (NbColonnes - col) * DeltaX, 0, Origine.Z - row * DeltaZ);
                    ++noSommet;
                }
            }
        }

        public Vector3 GetPointSpatial(int x, int y)
        {
            return Points[x, y];
        }

        private void CréerTableauPointsTexture()
        {
            int noSommet = 0;
            PointsTexture = new Vector2[NbColonnes + 1, NbRangées + 1];
            for (int row = 0; row < NbRangées + 1; ++row)
            {
                for (int col = 0; col < NbColonnes + 1; ++col)
                {
                    PointsTexture[col, row] = new Vector2((col) * DeltaTextureX, 1 - ((float)DeltaTextureY * row));
                    ++noSommet;
                }
            }
        }
        void GérerVisibilité()
        {
            if (CaméraJeu.Frustum.Intersects(SphereDeCollision))
            {
                EnableDraw = true;
            }
            else
            {
                EnableDraw = false;
            }
        }

        protected override void InitialiserSommets()
        {
            Sommets = new VertexPositionTexture[NbTrianglesDansTerrain * NB_SOMMETS_PAR_TRIANGLE];
            int noSommets = -1;

            for (int cptRow = 0; cptRow < NbColonnes; ++cptRow)
            {
                for (int cptCol = 0; cptCol < NbColonnes; ++cptCol)
                {
                    Sommets[++noSommets] = new VertexPositionTexture(Points[cptCol, cptRow], PointsTexture[cptCol, cptRow]);
                    Sommets[++noSommets] = new VertexPositionTexture(Points[cptCol, cptRow + 1], PointsTexture[cptCol, cptRow + 1]);
                    Sommets[++noSommets] = new VertexPositionTexture(Points[cptCol + 1, cptRow], PointsTexture[cptCol + 1, cptRow]);
                    Sommets[++noSommets] = new VertexPositionTexture(Points[cptCol, cptRow + 1], PointsTexture[cptCol, cptRow + 1]);
                    Sommets[++noSommets] = new VertexPositionTexture(Points[cptCol + 1, cptRow + 1], PointsTexture[cptCol + 1, cptRow + 1]);
                    Sommets[++noSommets] = new VertexPositionTexture(Points[cptCol + 1, cptRow], PointsTexture[cptCol + 1, cptRow]);
                }
            }
        }
        public override void Update(GameTime gameTime)
        {
            float TempsÉcoulé = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TempsÉcouléDepuisMAJ += TempsÉcoulé;
            if (TempsÉcouléDepuisMAJ >= IntervalleMAJ)
            {
                GérerVisibilité();
                TempsÉcouléDepuisMAJ = 0;
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            EffetDeBase.World = GetMonde();
            EffetDeBase.View = CaméraJeu.Vue;
            EffetDeBase.Projection = CaméraJeu.Projection;
            foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
            {
                passeEffet.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, Sommets, 0, NbTrianglesDansTerrain / NB_TRIANGLES_PAR_TUILE);
            }
            base.Draw(gameTime);
        }
    }
}
