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
    public class Voiture : ObjetDeBase
    {
        const float INCRÉMENT_ROTATION = (float)Math.PI / 720;
        Vector2 ÉtendueTotale { get; set; }
        const float FACTEUR_ACCÉLÉRATION = 3;
        const int INCRÉMENT_ANGLE = 10;
        const int RAYON_VOITURE = 10;
        float IntervalleMAJ { get; set; }
        Vector3 PositionCaméra { get; set; }
        Vector3 AncienneDirection { get; set; }
        Vector3 DirectionCaméra { get; set; }
        float NormeDirection { get; set; }
        float IntervalleAccélération { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }
        Réseautique GérerRéseau { get; set; }
        bool FirstTime { get; set; }
        Vector3 DirectionDérapage { get; set; }
        bool PremièreBoucleDérapage { get; set; }
        public float AngleVolant
        {
            get
            { return angleVolant; }
            private set
            {
                angleVolant = value;
                if (value < -0.3f) { angleVolant = -0.3f; }
                if (value > 0.3f) { angleVolant = 0.3f; }
            }
        }

        public float Vitesse
        {
            get
            { return vitesse; }
            private set
            {
                vitesse = value;
                if (value < -10) { vitesse = -10; }
                if (value > 500) { vitesse = 500; }
            }
        }
        public float TempsAccélération
        {
            get { return tempsAccélération; }
            private set
            {
                tempsAccélération = value;
                if (value < -2) { tempsAccélération = -2; }
                if (value > 20) { tempsAccélération = 20; }
            }
        }

        float vitesse;
        float tempsAccélération;
        float angleVolant;

        Vector3 Direction { get; set; }
        bool ChangementEffectué { get; set; }
        Caméra Caméra { get; set; }

        InputManager GestionInput { get; set; }
        bool DirectionModifiée { get; set; }

        public Voiture(Game jeu, String nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale)
        {
            IntervalleMAJ = intervalleMAJ;

            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            GérerRéseau = Game.Services.GetService(typeof(Réseautique)) as Réseautique;
            ÉtendueTotale = new Vector2(200 * 4, 200 * 4); //aller chercher de jeu
            IntervalleAccélération = 1f / 5f;
            Direction = new Vector3(-1, 0, 0);
            Vitesse = 0;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            Caméra = Game.Services.GetService(typeof(Caméra)) as Caméra;

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            float tempsÉcoulé = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TempsÉcouléDepuisMAJ += tempsÉcoulé;
            if (TempsÉcouléDepuisMAJ >= IntervalleMAJ)
            {
                VarierVitesse();
                CalculerVitesse();
                AjusterPosition();
                EffectuerTransformations();
                //RecréerMonde();
                Game.Window.Title = Vitesse.ToString("0.000");

                TempsÉcouléDepuisMAJ = 0;
            }

            base.Update(gameTime);
        }


        void VarierVitesse()
        {
            bool accélération = GestionInput.EstEnfoncée(Keys.W);
            bool freinage = GestionInput.EstEnfoncée(Keys.S);
            int signe = Math.Sign(Vitesse) == 0 ? 1 : Math.Sign(Vitesse);


            if ((!accélération && !freinage)) { TempsAccélération += (float)-signe / 2.0f * IntervalleAccélération; }
            if (accélération) { TempsAccélération += 2f * IntervalleAccélération; DirectionModifiée = true; }
            if (freinage) { TempsAccélération -= 3f * IntervalleAccélération; }

        }
        void CalculerVitesse()
        {
            Vitesse = FACTEUR_ACCÉLÉRATION * TempsAccélération;
        }
        int GérerTouche(Keys touche)
        {
            return GestionInput.EstEnfoncée(touche) ? INCRÉMENT_ANGLE : 0;
        }


        void AjusterPosition()
        {
            Direction = Vitesse * Vector3.Normalize(new Vector3(-(float)Math.Sin(Rotation.Y), 0, -(float)Math.Cos(Rotation.Y))) / 100f;
            //pédales + ajouter accélération??
            if(!GestionInput.EstEnfoncée(Keys.LeftShift))
            {
                Position += Direction;
                PremièreBoucleDérapage = true;
            }
            else
            {
                if(PremièreBoucleDérapage)
                {
                    DirectionDérapage = Direction;
                    PremièreBoucleDérapage = false;
                }
                else
                {
                     
                    Position += Vitesse * Vector3.Normalize(Direction + DirectionDérapage);
                }
            }
           
            
            ChangementEffectué = true;

            //Volant... degrés??
            if (GestionInput.EstEnfoncée(Keys.A) || GestionInput.EstEnfoncée(Keys.D))
            {
                int sens = GérerTouche(Keys.A) - GérerTouche(Keys.D);

                //Rotation = new Vector3(Rotation.X, Rotation.Y + sens * INCRÉMENT_ROTATION, Rotation.Z);
                Rotation = new Vector3(Rotation.X, Rotation.Y + sens * INCRÉMENT_ROTATION * Vitesse / 60f, Rotation.Z);
                ChangementEffectué = true;

            }
        }

        private void DéplacerCaméra()
        {
            if (DirectionModifiée)
            {
                if (Vitesse > 30)
                {
                    AncienneDirection = new Vector3(Direction.X, Direction.Y, Direction.Z);
                }

                CalculerPositionCaméra(Direction);
                Caméra.Déplacer(PositionCaméra, Position, Vector3.Up);
                DirectionModifiée = false;
            }
            else
            {
                CalculerPositionCaméra(AncienneDirection);
                Caméra.Déplacer(PositionCaméra, Position, Vector3.Up);
            }

        }

        void CalculerPositionCaméra(Vector3 direction)
        {
            if (Vitesse > 25)
            {
                PositionCaméra = Position - direction * Vitesse * 2 + new Vector3(0, 20, 0);
            }
            else
            {
                PositionCaméra = Position - direction * 25 * 2 + new Vector3(0, 20, 0);
            }

        }

        float CalculerPosition(int déplacement, float posActuelle)
        {
            return posActuelle + déplacement;
        }

        private void EffectuerTransformations()
        {
            if (ChangementEffectué)
            {
                GérerRéseau.SendMatriceMonde(Monde);
                //juste si le déplacement est good
                Monde = Matrix.CreateScale(Échelle) * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) * Matrix.CreateTranslation(Position);
                DéplacerCaméra();
                ChangementEffectué = false;
            }
        }
        public void AjusterPosition(Matrix nouvelleMatriceMonde)
        {
            Monde = nouvelleMatriceMonde;
        }
        public void RecréerMonde()
        {
            Monde = Matrix.Identity;
            Monde *= Matrix.CreateScale(Échelle);
            Monde *= Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            Monde *= Matrix.CreateTranslation(Position);
        }

    }
}
