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
    public class Section : Terrain
    {
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;
        Vector2 �tendue { get; set; }
        Vector2 Extr�mit� { get; set; }
        double Norme�tendue { get; set; }
        Maison Test { get; set; }
        
        List<GameComponent> Components { get; set; }
        public Section(Game game, Vector2 origine, Vector2 �tendue2,float homoth�tieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, Vector3 �tendue,
                       string[] nomsTexturesTerrain, float intervalleMAJ)
            : base(game, origine,homoth�tieInitiale, rotationInitiale, positionInitiale, �tendue, nomsTexturesTerrain, intervalleMAJ)
        {
            //�tendue = �tendue2;
            �tendue = new Vector2(�tendue.X, �tendue.Z);

        }

        public override void Initialize()
        {
            Extr�mit� = Coin + �tendue;
            Test = new Maison(Game, 10f, Vector3.Zero, new Vector3(Coin.X/* + �tendue.X*/, 0, Coin.Y/* - �tendue.Y*/), new Vector3(2,2,2), "brique1", "roof", 0.01f);
            Game.Components.Add(Test);
            Norme�tendue = Math.Sqrt(Math.Pow(�tendue.X, 2) + Math.Pow(�tendue.Y, 2));
            Cr�erPiste();
            base.Initialize();
        }
        //lol
        private void Cr�erPiste()
        {
            //Components.Add(new PisteSectionn�e(Game, 1f, Vector3.Zero, Vector3.Zero, INTERVALLE_MAJ_STANDARD, 20000, 20000, Coin, �tendue));
        }

        //public void AddComponent(GameComponent x)
        //{
        //    Components.Add(x);
        //}
    }
}
