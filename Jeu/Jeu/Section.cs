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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Section :Terrain
    {
        Vector2 Étendue { get; set; }
        Vector2 Extrémité { get; set; }
        double NormeÉtendue { get; set; }
        
        List<GameComponent> Components { get; set; }
        public Section(Game game, Vector2 origine, Vector2 étendue2,float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, Vector3 étendue,
                       string[] nomsTexturesTerrain, float intervalleMAJ)
            : base(game, origine,homothétieInitiale, rotationInitiale, positionInitiale, étendue, nomsTexturesTerrain, intervalleMAJ)
        {
            Étendue = étendue2;
        }

        public override void Initialize()
        {
            Extrémité = Coin + Étendue;
            NormeÉtendue = Math.Sqrt(Math.Pow(Étendue.X, 2) + Math.Pow(Étendue.Y, 2));
            base.Initialize();
        }

        public void AddComponent(GameComponent x)
        {
            Components.Add(x);
        }
    }
}
