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
        Vector3[] Points { get; set; }
        BasicEffect EffetDeBase { get; set; }
        Afficheur3D Afficheur { get; set; }
        public BannièreArrivée(Game game, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, Vector3[] points,float intervalleMAJ)
            : base(game, homothétieInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            Points = points;
        }
        protected override void InitialiserSommets()
        {
            Sommets = new VertexPositionColor[4];
            Sommets[0] = new VertexPositionColor(Points[0], Color.White);
            Sommets[1] = new VertexPositionColor(Points[1], Color.White);
            Sommets[2] = new VertexPositionColor(Points[2], Color.White);
            Sommets[3] = new VertexPositionColor(Points[3], Color.White);


        }
        public override void Initialize()
        {
            base.Initialize();
        }
        protected override void LoadContent()
        {
            EffetDeBase = new BasicEffect(GraphicsDevice);
            EffetDeBase.TextureEnabled = false;
            EffetDeBase.VertexColorEnabled = true;
            base.LoadContent();
        }
        public override void Draw(GameTime gameTime)
        {
            DepthStencilState ancienDepthStencilState = GraphicsDevice.DepthStencilState;
            DepthStencilState temporaire = new DepthStencilState();
            temporaire.DepthBufferEnable = false;
            GraphicsDevice.DepthStencilState = temporaire;

            EffetDeBase.World = GetMonde();
            EffetDeBase.View = CaméraJeu.Vue;
            EffetDeBase.Projection = CaméraJeu.Projection;
            foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
            {
                //const
                passeEffet.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, Sommets, 0, 2);
            }
            GraphicsDevice.DepthStencilState = ancienDepthStencilState;
        }
        public bool EstÀArrivée(Vector3 position)
        {
            float minX = Points[0].X;
            float minZ = Points[0].Z;
            float maxX = Points[0].X;
            float maxZ = Points[0].Z;
            for (int i = 1; i < 4; ++i)
            {
                if (Points[i].X < minX)
                {
                    minX = Points[i].X;
                }
                else
                {
                    if(Points[i].X > maxX)
                    {
                        maxX = Points[i].X;
                    }
                }
                if(Points[i].Z < minZ)
                {
                    minZ = Points[i].Z;
                }
                else
                {
                    if(Points[i].Z > maxZ)
                    {
                        maxZ = Points[i].Z;
                    }
                }
            }
            return position.X < maxX && position.X > minX && position.Z > minZ && position.Z < maxZ ;
        }
    }
}

