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
        DataPiste Données { get; set; }
        BasicEffect EffetDeBase { get; set; }
        Afficheur3D Afficheur { get; set; }
        public BannièreArrivée(Game game, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale,float intervalleMAJ)
            : base(game, homothétieInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {

        }
        protected override void InitialiserSommets()
        {
            List<Vector2> ext = Données.GetBordureExtérieure();
            List<Vector2> inté = Données.GetBordureIntérieur();
            Sommets = new VertexPositionColor[4];
            Points = new Vector3[4];
            Points[0] = new Vector3(inté[0].X, 0, inté[0].Y);
            Points[1] = new Vector3(inté[inté.Count - 1].X, 0, inté[inté.Count - 1].Y);
            Points[2] = new Vector3(ext[0].X, 0, ext[0].Y);
            Points[3] = new Vector3(ext[ext.Count - 1].X, 0, ext[ext.Count - 1].Y);
            Sommets[0] = new VertexPositionColor(Points[0], Color.White);
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
                passeEffet.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, Sommets, 0, 2);
            }
            //GraphicsDevice.DepthStencilState = ancienDepthStencilState;
            //GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
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

