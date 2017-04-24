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
    public class BannièreArrivée : PrimitiveDeBaseAnimée
    {
        VertexPositionColor[] Sommets { get; set; }
        DataPiste Données { get; set; }
        BasicEffect EffetDeBase { get; set; }
        Afficheur3D Afficheur { get; set; }
        public BannièreArrivée(Game game, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ)
            : base(game, homothétieInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {

        }
        protected override void InitialiserSommets()
        {
            List<Vector2> ext = Données.GetBordureExtérieure();
            List<Vector2> inté = Données.GetBordureIntérieur();
            Sommets = new VertexPositionColor[4];
            Sommets[0] = new VertexPositionColor(new Vector3(inté[0].X, 0, inté[0].Y), Color.White);
            Sommets[1] = new VertexPositionColor(new Vector3(inté[inté.Count - 1].X, 0, inté[inté.Count - 1].Y), Color.White);
            Sommets[2] = new VertexPositionColor(new Vector3(ext[0].X, 0, ext[0].Y), Color.White);
            Sommets[3] = new VertexPositionColor(new Vector3(ext[ext.Count - 1].X, 0, ext[ext.Count - 1].Y), Color.White);



        }
        public override void Initialize()
        {
            DrawOrder = 1; //NO
            Données = Game.Services.GetService(typeof(DataPiste)) as DataPiste;
            base.Initialize();
        }
        protected override void LoadContent()
        {
            EffetDeBase = new BasicEffect(GraphicsDevice);
            EffetDeBase.TextureEnabled = false;
            EffetDeBase.VertexColorEnabled = true;
            base.LoadContent();
        }
        //public override void Update(GameTime gameTime)
        //{
        //    base.Update(gameTime);
        //}
        public override void Draw(GameTime gameTime)
        {
            //DepthStencilState ancienDepthStencilState = GraphicsDevice.DepthStencilState;
            //DepthStencilState temporaire = new DepthStencilState();
            //temporaire.DepthBufferEnable = false;
            //GraphicsDevice.DepthStencilState = temporaire;
            //GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            //EffetDeBase.TextureEnabled = false;
            //EffetDeBase.VertexColorEnabled = true;
            EffetDeBase.World = GetMonde();
            EffetDeBase.View = CaméraJeu.Vue;
            EffetDeBase.Projection = CaméraJeu.Projection;
            foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
            {
                //const
                //GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, Sommets, 0, 2);
            }
            //GraphicsDevice.DepthStencilState = ancienDepthStencilState;
            //GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }
    }
}
