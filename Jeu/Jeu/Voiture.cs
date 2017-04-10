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
    public class Voiture : ObjetDeBase, ICollisionable, IResettable
    {
        //constantes

        const int VITESSE_MAX = 200;
        const int VITESSE_MIN = -5;
        const int TEMPS_ACCÉLÉRATION_MAX = 50;
        const float INCRÉMENT_ROTATION = (float)Math.PI / 1440;
        const float COEFFICIENT_FROTTEMENT_GOMME_PNEU_ASPHALTE = 0.8f;
        const float INTERVALLE_RALENTISSEMENT = 1f / 5f;
        const int DISTANCE_CAMÉRA = 400;

        // propriétés
        Vector2 ÉtendueTotale { get; set; }
        const float FACTEUR_ACCÉLÉRATION = 1f / 5f;
        const int INCRÉMENT_ANGLE = 10;
        const float RAYON_VOITURE = 0.1f;
        float IntervalleMAJ { get; set; }
        Vector3 PositionCaméra { get; set; }
        Vector3 DirectionCaméra { get; set; }
        float NormeDirection { get; set; }
        float intervalleAccélération;
        float IntervalleAccélération
        {
            get
            {
                if (TempsAccélération < 10)
                {
                    return 6f;
                }
                else if (TempsAccélération < 20)
                {
                    return 5f;
                }
                else if (TempsAccélération >= 20)
                {
                    return 4f;
                }
                return intervalleAccélération;

            }
            set { intervalleAccélération = value; }
        }
        float TempsÉcouléDepuisMAJ { get; set; }
        Réseautique GérerRéseau { get; set; }
        Vector3 DirectionDérapage { get; set; }
        bool PremièreBoucleDérapage { get; set; }
        public bool EstActif { get; set; }
        public BoundingSphere SphèreDeCollision { get; set; }
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
                if (value < VITESSE_MIN) { vitesse = VITESSE_MIN; }
                if (value > VITESSE_MAX) { vitesse = VITESSE_MAX; }

            }
        }
        public float TempsAccélération
        {
            get { return tempsAccélération; }
            private set
            {
                tempsAccélération = value;
                if (value < -20) { tempsAccélération = -20; }
                if (value > TEMPS_ACCÉLÉRATION_MAX) { tempsAccélération = TEMPS_ACCÉLÉRATION_MAX; }
            }
        }

        float vitesse;
        float tempsAccélération;
        float angleVolant;

        Vector3 Direction { get; set; }
        bool ChangementEffectué { get; set; }
        Caméra Caméra { get; set; }

        InputManager GestionInput { get; set; }
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
            IntervalleAccélération = 1f / 10f;
            Direction = new Vector3(0, 0, 75);
            Vitesse = 0;
            base.Initialize();
            SphèreDeCollision = new BoundingSphere(Position, Norme(Monde.Forward, Monde.Backward));
            DirectionCaméra = Monde.Forward - Monde.Backward;
            DéplacerCaméra();
        }

        float Norme(Vector3 x, Vector3 y)
        {
            Vector3 z = x - y;
            return (float)Math.Sqrt(Math.Pow(z.X, 2) + Math.Pow(z.Y, 2) + Math.Pow(z.Z, 2));
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
            if (TempsÉcouléDepuisMAJ >= IntervalleMAJ && EstActif)
            {
                VarierVitesse();
                CalculerVitesse();
                AjusterPosition();
                EffectuerTransformations();
                //RecréerMonde();
                Game.Window.Title = "Position : " + Position.X.ToString("0.0") + " / " + Position.Y.ToString("0.0") + " / " + Position.Z.ToString("0.0") + " Vitesse : " + Vitesse.ToString("0.0") + " / TempsAccélaration" + TempsAccélération.ToString("0.0");
                SphèreDeCollision = new BoundingSphere(Monde.Translation, 12f);
                TempsÉcouléDepuisMAJ = 0;
            }

            base.Update(gameTime);
        }


        void VarierVitesse()
        {
            bool accélération = GestionInput.EstEnfoncée(Keys.W);
            bool freinage = GestionInput.EstEnfoncée(Keys.S);
            int signe = Math.Sign(Vitesse) == 0 ? 1 : Math.Sign(Vitesse);


            if ((!accélération && !freinage)) { TempsAccélération += (float)-signe / 2.0f * INTERVALLE_RALENTISSEMENT; }
            if (accélération) { TempsAccélération += 0.5f * FACTEUR_ACCÉLÉRATION; }
            if (freinage) { TempsAccélération -= 3f * INTERVALLE_RALENTISSEMENT; }

        }
        void CalculerVitesse()
        {
            Vitesse = IntervalleAccélération * TempsAccélération;
        }
        int GérerTouche(Keys touche)
        {
            return GestionInput.EstEnfoncée(touche) ? INCRÉMENT_ANGLE : 0;
        }


        void AjusterPosition()
        {
            Direction = Vitesse * Vector3.Normalize(new Vector3(-(float)Math.Sin(Rotation.Y), 0, -(float)Math.Cos(Rotation.Y))) / 100f;
            //pédales + ajouter accélération??
            if (!GestionInput.EstEnfoncée(Keys.LeftControl))
            {
                Position += Direction;
                PremièreBoucleDérapage = true;
            }
            else
            {
                if (PremièreBoucleDérapage)
                {
                    DirectionDérapage = new Vector3(Direction.X, Direction.Y, Direction.Z);
                    PremièreBoucleDérapage = false;
                }
                else
                {
                    DirectionDérapage = Vitesse * Vector3.Normalize(DirectionDérapage) / 100f;
                    Direction = Vitesse * Vector3.Normalize(Direction) / 100f;
                    Position += COEFFICIENT_FROTTEMENT_GOMME_PNEU_ASPHALTE * (Direction + DirectionDérapage) / 2;
                    if (TempsAccélération <= 0)
                    {
                        TempsAccélération = 0;
                    }
                    if (TempsAccélération > 0)
                    {
                        TempsAccélération -= IntervalleMAJ * TempsAccélération;

                    }
                    if (GestionInput.EstEnfoncée(Keys.Tab))
                    {
                        Position += Direction;
                    }
                }
            }


            ChangementEffectué = true;

            //Volant... degrés??
            if (GestionInput.EstEnfoncée(Keys.A) || GestionInput.EstEnfoncée(Keys.D))
            {
                int sens = GérerTouche(Keys.A) - GérerTouche(Keys.D);
                if(!GestionInput.EstEnfoncée(Keys.LeftControl))
                {
                    if (TempsAccélération > 0)
                    {
                        TempsAccélération -= INTERVALLE_RALENTISSEMENT / 10;
                    }
                    if (TempsAccélération <= 0)
                    {
                        TempsAccélération = 0;
                    }
                }
                //Rotation = new Vector3(Rotation.X, Rotation.Y + sens * INCRÉMENT_ROTATION, Rotation.Z);
                Rotation = new Vector3(Rotation.X, Rotation.Y + sens * Vitesse / 100f * INCRÉMENT_ROTATION, Rotation.Z);
                ChangementEffectué = true;

            }
        }

        private void DéplacerCaméra()
        {
            CalculerPositionCaméra();
            DirectionCaméra = Monde.Forward - Monde.Backward;
            Caméra.Déplacer(PositionCaméra, Position, Vector3.Up);
        }

        void CalculerPositionCaméra()
        {
            if (GestionInput.EstEnfoncée(Keys.LeftAlt)) 
            {
                DirectionCaméra = -DirectionCaméra;
            }
            PositionCaméra = Position - (DirectionCaméra) * DISTANCE_CAMÉRA + new Vector3(0, DISTANCE_CAMÉRA/2 * Échelle, 0);
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
            SphèreDeCollision = new BoundingSphere(Monde.Translation, 12f); //po legit?
        }
        public void RecréerMonde()
        {
            Monde = Matrix.Identity;
            Monde *= Matrix.CreateScale(Échelle);
            Monde *= Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            Monde *= Matrix.CreateTranslation(Position);
        }

        public bool EstEnCollision(object autreObjet)
        {
            bool valeurRetour = false;
            if (autreObjet is ICollisionable)
            {
                valeurRetour = SphèreDeCollision.Intersects((autreObjet as ICollisionable).SphèreDeCollision);
            }
            return valeurRetour;
        }
        public void Rebondir()
        {
            TempsAccélération = -TempsAccélération;
            //Je sais ca marche po lolo
        }
    }
}
