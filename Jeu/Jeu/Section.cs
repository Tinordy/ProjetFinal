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
        Vector2 Étendue { get; set; }
        Vector2 Extrémité { get; set; }
        double NormeÉtendue { get; set; }
        PisteSectionnée Piste { get; set; }

        List<DrawableGameComponent> Components { get; set; }
        public Section(Game game, Vector2 origine, Vector2 étendue2, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, Vector3 étendue,
                       string[] nomsTexturesTerrain, float intervalleMAJ)
            : base(game, origine, homothétieInitiale, rotationInitiale, positionInitiale, étendue, nomsTexturesTerrain, intervalleMAJ)
        {
            //Étendue = étendue2;
            Étendue = new Vector2(étendue.X, étendue.Z);

        }

        public override void Initialize()
        {
            Components = new List<DrawableGameComponent>();
            Extrémité = Coin + Étendue;
            NormeÉtendue = Math.Sqrt(Math.Pow(Étendue.X, 2) + Math.Pow(Étendue.Y, 2));
            CréerPiste();
            CréerMaisons();
            base.Initialize();
        }

        private void CréerMaisons()
        {
            if (Piste.NbPtsCentraux > 0)
            {
                List<Vector2> vecteursPerpendiculaires = Piste.ObtenirVecteurPerp();
                for(int i = 0; i < vecteursPerpendiculaires.Count; i += 2)
                {
                    Vector2 temp = 4 * vecteursPerpendiculaires[i] + vecteursPerpendiculaires[i+1];
                    Maison maison = new Maison(Game, 5f, Vector3.Zero, new Vector3(temp.X, 0, temp.Y), new Vector3(2, 2, 2), "brique1", "roof", 0.01f);
                    Components.Add(maison);
                    Game.Components.Add(maison);
                }
            }

        }
        private void CréerPiste()
        {

            Piste = new PisteSectionnée(Game,1 , Vector3.Zero, Vector3.Zero, INTERVALLE_MAJ_STANDARD, 20000, 20000, Coin, Étendue);
            Piste.Initialize();
        
            Components.Add(Piste);
            Game.Components.Add(Piste);
        }
        protected override void OnVisibleChanged(object sender, EventArgs args)
        {
            foreach (DrawableGameComponent c in Components)
            {
                c.Visible = Visible;
            }
        }
        public bool EstEnCollisionAvecUnObjet(ICollisionable objet)
        {
            bool valRetour = false;
            int i = 0;
            while(!valRetour && i < Components.Count)
            {
                if(objet.EstEnCollision(Components[i]))
                {

                }
                valRetour = objet.EstEnCollision(Components[i]);
                ++i;
            }
            return valRetour;
        }
        public PisteSectionnée ObtenirSectionPiste()
        {
            return Piste;
        }
        //public void AddComponent(GameComponent x)
        //{
        //    Components.Add(x);
        //}
    }
}
