using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace AtelierXNA
{
   public class CubeColoré : PrimitiveDeBaseAnimée
   {

      const int NB_SOMMETS = 16;
      const int NB_TRIANGLES = 12;
      Color Couleur { get; set; }
      VertexPositionColor[] Sommets { get; set; }
      Vector3 Origine { get; set; }
      float DeltaX { get; set; }
      float DeltaY { get; set; }
      float DeltaZ { get; set; }
      BasicEffect EffetDeBase { get; set; }
      public CubeColoré(Game game, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, Color couleur, Vector3 dimension, float intervalleMAJ)
       : base(game, homothétieInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
      {
         Couleur = couleur;
         DeltaX = dimension.X;
         DeltaY = dimension.Y;
         DeltaZ = dimension.Z;
         Origine = new Vector3(-DeltaX / 2, -DeltaY / 2, DeltaZ / 2);
      }

      public override void Initialize()
      {
         Sommets = new VertexPositionColor[NB_SOMMETS];
         base.Initialize();
      }

      protected override void LoadContent()
      {
         EffetDeBase = new BasicEffect(GraphicsDevice);
         EffetDeBase.VertexColorEnabled = true;
         base.LoadContent();
      }

      protected override void InitialiserSommets()
      {
         const int NB_POINTS = 8;
         Vector3[] tabPts = new Vector3[NB_POINTS];
         InitialiserTabPoints(tabPts);
         int cpt = 0;
         while(cpt<NB_POINTS)
         {
            Sommets[cpt] = new VertexPositionColor(tabPts[cpt], Color.Orchid);
            ++cpt;
         }
         Sommets[cpt++] = new VertexPositionColor(tabPts[2], Couleur);
         Sommets[cpt++] = new VertexPositionColor(tabPts[4], Couleur);
         Sommets[cpt++] = new VertexPositionColor(tabPts[0], Couleur);
         Sommets[cpt++] = new VertexPositionColor(tabPts[6], Couleur);
         Sommets[cpt++] = new VertexPositionColor(tabPts[1], Couleur);
         Sommets[cpt++] = new VertexPositionColor(tabPts[7], Couleur);
         Sommets[cpt++] = new VertexPositionColor(tabPts[3], Couleur);
         Sommets[cpt] = new VertexPositionColor(tabPts[5], Couleur);
      }
      void InitialiserTabPoints(Vector3[] tab)
      {
         int i = 0;
         tab[i++] = Origine;
         tab[i++] = new Vector3(Origine.X + DeltaX, Origine.Y, Origine.Z);
         tab[i++] = new Vector3(Origine.X, Origine.Y, Origine.Z - DeltaY);
         tab[i++] = new Vector3(Origine.X + DeltaX, Origine.Y, Origine.Z - DeltaZ);
         tab[i++] = new Vector3(Origine.X, Origine.Y + DeltaY, Origine.Z - DeltaZ);
         tab[i++] = new Vector3(Origine.X + DeltaX, Origine.Y + DeltaY, Origine.Z - DeltaZ);
         tab[i++] = new Vector3(Origine.X, Origine.Y + DeltaY, Origine.Z);
         tab[i++] = new Vector3(Origine.X + DeltaX, Origine.Y + DeltaY, Origine.Z);
      }
      public override void Draw(GameTime gameTime)
      {
         EffetDeBase.World = GetMonde();
         EffetDeBase.View = CaméraJeu.Vue;
         EffetDeBase.Projection = CaméraJeu.Projection;
         foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
         {
            passeEffet.Apply();
            GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, Sommets, 0, NB_TRIANGLES / 2);
            GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, Sommets, NB_SOMMETS / 2, NB_TRIANGLES / 2);
         }
      }
   }
}