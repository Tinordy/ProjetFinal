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
        const int TEMPS_ACC�L�RATION_MAX = 50;
        const float INCR�MENT_ROTATION = (float)Math.PI / 1440;
        const float COEFFICIENT_FROTTEMENT_GOMME_PNEU_ASPHALTE = 0.8f;
        const float INTERVALLE_RALENTISSEMENT = 1f / 5f;
        const int DISTANCE_CAM�RA = 400;

        // propri�t�s
        Vector2 �tendueTotale { get; set; }
        const float FACTEUR_ACC�L�RATION = 1f / 5f;
        const int INCR�MENT_ANGLE = 10;
        const float RAYON_VOITURE = 0.1f;
        float IntervalleMAJ { get; set; }
        Vector3 PositionCam�ra { get; set; }
        Vector3 DirectionCam�ra { get; set; }
        float NormeDirection { get; set; }
        float intervalleAcc�l�ration;
        float IntervalleAcc�l�ration
        {
            get
            {
                if (TempsAcc�l�ration < 10)
                {
                    return 6f;
                }
                else if (TempsAcc�l�ration < 20)
                {
                    return 5f;
                }
                else if (TempsAcc�l�ration >= 20)
                {
                    return 4f;
                }
                return intervalleAcc�l�ration;

            }
            set { intervalleAcc�l�ration = value; }
        }
        float Temps�coul�DepuisMAJ { get; set; }
        R�seautique G�rerR�seau { get; set; }
        Vector3 DirectionD�rapage { get; set; }
        bool Premi�reBoucleD�rapage { get; set; }
        public bool EstActif { get; set; }
        public BoundingSphere Sph�reDeCollision { get; set; }
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
        public float TempsAcc�l�ration
        {
            get { return tempsAcc�l�ration; }
            private set
            {
                tempsAcc�l�ration = value;
                if (value < -20) { tempsAcc�l�ration = -20; }
                if (value > TEMPS_ACC�L�RATION_MAX) { tempsAcc�l�ration = TEMPS_ACC�L�RATION_MAX; }
            }
        }

        float vitesse;
        float tempsAcc�l�ration;
        float angleVolant;

        Vector3 Direction { get; set; }
        bool ChangementEffectu� { get; set; }
        Cam�ra Cam�ra { get; set; }

        InputManager GestionInput { get; set; }
        public Voiture(Game jeu, String nomMod�le, float �chelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ)
            : base(jeu, nomMod�le, �chelleInitiale, rotationInitiale, positionInitiale)
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

            G�rerR�seau = Game.Services.GetService(typeof(R�seautique)) as R�seautique;
            �tendueTotale = new Vector2(200 * 4, 200 * 4); //aller chercher de jeu
            IntervalleAcc�l�ration = 1f / 10f;
            Direction = new Vector3(0, 0, 75);
            Vitesse = 0;
            base.Initialize();
            Sph�reDeCollision = new BoundingSphere(Position, Norme(Monde.Forward, Monde.Backward));
            DirectionCam�ra = Monde.Forward - Monde.Backward;
            D�placerCam�ra();
        }

        float Norme(Vector3 x, Vector3 y)
        {
            Vector3 z = x - y;
            return (float)Math.Sqrt(Math.Pow(z.X, 2) + Math.Pow(z.Y, 2) + Math.Pow(z.Z, 2));
        }

        protected override void LoadContent()
        {
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            Cam�ra = Game.Services.GetService(typeof(Cam�ra)) as Cam�ra;

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            float temps�coul� = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Temps�coul�DepuisMAJ += temps�coul�;
            if (Temps�coul�DepuisMAJ >= IntervalleMAJ && EstActif)
            {
                VarierVitesse();
                CalculerVitesse();
                AjusterPosition();
                EffectuerTransformations();
                //Recr�erMonde();
                Game.Window.Title = "Position : " + Position.X.ToString("0.0") + " / " + Position.Y.ToString("0.0") + " / " + Position.Z.ToString("0.0") + " Vitesse : " + Vitesse.ToString("0.0") + " / TempsAcc�laration" + TempsAcc�l�ration.ToString("0.0");
                Sph�reDeCollision = new BoundingSphere(Monde.Translation, 12f);
                Temps�coul�DepuisMAJ = 0;
            }

            base.Update(gameTime);
        }


        void VarierVitesse()
        {
            bool acc�l�ration = GestionInput.EstEnfonc�e(Keys.W);
            bool freinage = GestionInput.EstEnfonc�e(Keys.S);
            int signe = Math.Sign(Vitesse) == 0 ? 1 : Math.Sign(Vitesse);


            if ((!acc�l�ration && !freinage)) { TempsAcc�l�ration += (float)-signe / 2.0f * INTERVALLE_RALENTISSEMENT; }
            if (acc�l�ration) { TempsAcc�l�ration += 0.5f * FACTEUR_ACC�L�RATION; }
            if (freinage) { TempsAcc�l�ration -= 3f * INTERVALLE_RALENTISSEMENT; }

        }
        void CalculerVitesse()
        {
            Vitesse = IntervalleAcc�l�ration * TempsAcc�l�ration;
        }
        int G�rerTouche(Keys touche)
        {
            return GestionInput.EstEnfonc�e(touche) ? INCR�MENT_ANGLE : 0;
        }


        void AjusterPosition()
        {
            Direction = Vitesse * Vector3.Normalize(new Vector3(-(float)Math.Sin(Rotation.Y), 0, -(float)Math.Cos(Rotation.Y))) / 100f;
            //p�dales + ajouter acc�l�ration??
            if (!GestionInput.EstEnfonc�e(Keys.LeftControl))
            {
                Position += Direction;
                Premi�reBoucleD�rapage = true;
            }
            else
            {
                if (Premi�reBoucleD�rapage)
                {
                    DirectionD�rapage = new Vector3(Direction.X, Direction.Y, Direction.Z);
                    Premi�reBoucleD�rapage = false;
                }
                else
                {
                    DirectionD�rapage = Vitesse * Vector3.Normalize(DirectionD�rapage) / 100f;
                    Direction = Vitesse * Vector3.Normalize(Direction) / 100f;
                    Position += COEFFICIENT_FROTTEMENT_GOMME_PNEU_ASPHALTE * (Direction + DirectionD�rapage) / 2;
                    if (TempsAcc�l�ration <= 0)
                    {
                        TempsAcc�l�ration = 0;
                    }
                    if (TempsAcc�l�ration > 0)
                    {
                        TempsAcc�l�ration -= IntervalleMAJ * TempsAcc�l�ration;

                    }
                    if (GestionInput.EstEnfonc�e(Keys.Tab))
                    {
                        Position += Direction;
                    }
                }
            }


            ChangementEffectu� = true;

            //Volant... degr�s??
            if (GestionInput.EstEnfonc�e(Keys.A) || GestionInput.EstEnfonc�e(Keys.D))
            {
                int sens = G�rerTouche(Keys.A) - G�rerTouche(Keys.D);
                if(!GestionInput.EstEnfonc�e(Keys.LeftControl))
                {
                    if (TempsAcc�l�ration > 0)
                    {
                        TempsAcc�l�ration -= INTERVALLE_RALENTISSEMENT / 10;
                    }
                    if (TempsAcc�l�ration <= 0)
                    {
                        TempsAcc�l�ration = 0;
                    }
                }
                //Rotation = new Vector3(Rotation.X, Rotation.Y + sens * INCR�MENT_ROTATION, Rotation.Z);
                Rotation = new Vector3(Rotation.X, Rotation.Y + sens * Vitesse / 100f * INCR�MENT_ROTATION, Rotation.Z);
                ChangementEffectu� = true;

            }
        }

        private void D�placerCam�ra()
        {
            CalculerPositionCam�ra();
            DirectionCam�ra = Monde.Forward - Monde.Backward;
            Cam�ra.D�placer(PositionCam�ra, Position, Vector3.Up);
        }

        void CalculerPositionCam�ra()
        {
            if (GestionInput.EstEnfonc�e(Keys.LeftAlt)) 
            {
                DirectionCam�ra = -DirectionCam�ra;
            }
            PositionCam�ra = Position - (DirectionCam�ra) * DISTANCE_CAM�RA + new Vector3(0, DISTANCE_CAM�RA/2 * �chelle, 0);
        }

        float CalculerPosition(int d�placement, float posActuelle)
        {
            return posActuelle + d�placement;
        }

        private void EffectuerTransformations()
        {
            if (ChangementEffectu�)
            {
                G�rerR�seau.SendMatriceMonde(Monde);
                //juste si le d�placement est good
                Monde = Matrix.CreateScale(�chelle) * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) * Matrix.CreateTranslation(Position);
                D�placerCam�ra();
                ChangementEffectu� = false;
            }
        }
        public void AjusterPosition(Matrix nouvelleMatriceMonde)
        {
            Monde = nouvelleMatriceMonde;
            Sph�reDeCollision = new BoundingSphere(Monde.Translation, 12f); //po legit?
        }
        public void Recr�erMonde()
        {
            Monde = Matrix.Identity;
            Monde *= Matrix.CreateScale(�chelle);
            Monde *= Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            Monde *= Matrix.CreateTranslation(Position);
        }

        public bool EstEnCollision(object autreObjet)
        {
            bool valeurRetour = false;
            if (autreObjet is ICollisionable)
            {
                valeurRetour = Sph�reDeCollision.Intersects((autreObjet as ICollisionable).Sph�reDeCollision);
            }
            return valeurRetour;
        }
        public void Rebondir()
        {
            TempsAcc�l�ration = -TempsAcc�l�ration;
            //Je sais ca marche po lolo
        }
    }
}
