using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AtelierXNA
{
    class Maison : PrimitiveDeBaseAnimée
    {
        const int NB_TRIANGLES_MURS = 8;
        const int NB_SOMMETS_PAR_TRIANGLE = 3;
        const int NB_TRIANGLES_TOIT = 4;

        int NbSommetsMurs { get; set; }
        int NbSommetsToit { get; set; }
        Vector3 Origine { get; set; }
        VertexPositionTexture[] SommetsMurs { get; set; }
        VertexPositionTexture[] SommetsToit { get; set; }
        Vector2[] PtsTextureMurs { get; set; }
        Vector2[] PtsTextureToit { get; set; }
        Vector3 Étendue { get; set; }
        BlendState GestionAlpha { get; set; }
        string NomTextureMurs { get; set; }
        Texture2D TextureMurs;
        string NomTextureToit { get; set; }
        Texture2D TextureToit { get; set; }
        RessourcesManager<Texture2D> gestionnaireDeTextures;
        protected BasicEffect EffetDeBase { get; private set; }
        public bool nullité { get; set; }
        InputManager GestionInput { get; set; }

        //constructeur vide pour créer un objet "inexistant"
        public Maison(Game jeu, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ)
            : base(jeu, homothétieInitiale, rotationInitiale, positionInitiale, intervalleMAJ) { Enabled = false; }

        public Maison(Game jeu, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, Vector3 étendue, string nomTextureMurs, string nomTextureToit, float intervalleMAJ)
    : base(jeu, échelleInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            NomTextureMurs = nomTextureMurs;
            NomTextureToit = nomTextureToit;
            Étendue = étendue;
            Origine = new Vector3(-Étendue.X / 2, 0, Étendue.Z / 2);//sur le sol
        }
        public override void Initialize()
        {
            NbSommets = NB_TRIANGLES_MURS + 2;
            NbSommetsMurs = NB_TRIANGLES_MURS + 2;
            NbSommetsToit = NB_TRIANGLES_TOIT * NB_SOMMETS_PAR_TRIANGLE;
            CréerTableauSommets();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            EffetDeBase = new BasicEffect(GraphicsDevice);
            gestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            TextureMurs = gestionnaireDeTextures.Find(NomTextureMurs);
            TextureToit = gestionnaireDeTextures.Find(NomTextureToit);
            InitialiserParamètresEffetDeBase();
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            EffetDeBase.World = GetMonde();
            EffetDeBase.View = CaméraJeu.Vue;
            EffetDeBase.Projection = CaméraJeu.Projection;
            foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
            {
                EffetDeBase.Texture = TextureMurs;
                passeEffet.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, SommetsMurs, 0, NB_TRIANGLES_MURS);
                EffetDeBase.Texture = TextureToit;
                passeEffet.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, SommetsToit, 0, NB_TRIANGLES_TOIT);
            }
            base.Draw(gameTime);
        }
        private void CréerTableauSommets()
        {
            PtsTextureMurs = new Vector2[NbSommetsMurs];
            SommetsMurs = new VertexPositionTexture[NbSommetsMurs];
            PtsTextureToit = new Vector2[NB_SOMMETS_PAR_TRIANGLE];
            SommetsToit = new VertexPositionTexture[NbSommetsToit];
            CréerTableauPointsTexture();
        }
        private void CréerTableauPointsTexture()
        {
            //points murs
            for (int i = 0; i < NbSommetsMurs; ++i)
            {
                PtsTextureMurs[i] = new Vector2((float)(i / 2) / 4, (i + 1) % 2);
            }
            //points toit
            PtsTextureToit[0] = new Vector2(0, 1);
            PtsTextureToit[1] = new Vector2(0.5f, 0);
            PtsTextureToit[2] = new Vector2(1, 1);
        }
        protected override void InitialiserSommets()
        {
            //sommets murs
            int i = 0;
            SommetsMurs[i] = new VertexPositionTexture(Origine, PtsTextureMurs[i++]);
            SommetsMurs[i] = new VertexPositionTexture(new Vector3(Origine.X, Origine.Y + Étendue.Y, Origine.Z), PtsTextureMurs[i++]);
            while (i < NbSommetsMurs)
            {
                SommetsMurs[i] = new VertexPositionTexture(new Vector3(Origine.X + (1 - i / 6) * Étendue.X, Origine.Y + (i % 2) * Étendue.Y, Origine.Z - (i / 4) % 2 * Étendue.Z), PtsTextureMurs[i]);
                ++i;
            }
            i = 0;

            //sommets toit
            SommetsToit[i] = new VertexPositionTexture(new Vector3(Origine.X, Origine.Y + Étendue.Y, Origine.Z), PtsTextureMurs[i % 3]);
            i++;
            while (i < NbSommetsToit)
            {
                if ((i - 1) % 3 == 0)
                {
                    SommetsToit[i] = new VertexPositionTexture(new Vector3(Origine.X + Étendue.X / 2, Origine.Y + 3f / 2 * Étendue.Y, Origine.Z - Étendue.Z / 2), PtsTextureToit[i % 3]);
                }
                else
                {
                    SommetsToit[i] = new VertexPositionTexture(new Vector3(Origine.X + (1 - (i / 8)) * Étendue.X, Origine.Y + Étendue.Y, Origine.Z - ((i / 5) % 2) * Étendue.Z), PtsTextureToit[i % 3]);

                }
                i++;
            }
        }
        private void InitialiserParamètresEffetDeBase()
        {
            EffetDeBase.TextureEnabled = true;
            //EffetDeBase.Texture = TextureMurs;
            GestionAlpha = BlendState.AlphaBlend;
        }
        public override void Update(GameTime gameTime)
        {
            CalculerMatriceMonde();
            base.Update(gameTime);
        }
    }

}
