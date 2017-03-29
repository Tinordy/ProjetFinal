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
        const float INCR�MENT_ROTATION = (float)Math.PI / 720;
        Vector2 �tendueTotale { get; set; }
        const float FACTEUR_ACC�L�RATION = 3;
        const int INCR�MENT_ANGLE = 10;
        const int RAYON_VOITURE = 10;
        float IntervalleMAJ { get; set; }
        Vector3 PositionCam�ra { get; set; }
        Vector3 AncienneDirection { get; set; }
        Vector3 DirectionCam�ra { get; set; }
        float NormeDirection { get; set; }
        float IntervalleAcc�l�ration { get; set; }
        float Temps�coul�DepuisMAJ { get; set; }
        R�seautique G�rerR�seau { get; set; }
        bool FirstTime { get; set; }
        Vector3 DirectionD�rapage { get; set; }
        bool Premi�reBoucleD�rapage { get; set; }
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
        public float TempsAcc�l�ration
        {
            get { return tempsAcc�l�ration; }
            private set
            {
                tempsAcc�l�ration = value;
                if (value < -2) { tempsAcc�l�ration = -2; }
                if (value > 20) { tempsAcc�l�ration = 20; }
            }
        }

        float vitesse;
        float tempsAcc�l�ration;
        float angleVolant;

        Vector3 Direction { get; set; }
        bool ChangementEffectu� { get; set; }
        Cam�ra Cam�ra { get; set; }

        InputManager GestionInput { get; set; }
        bool DirectionModifi�e { get; set; }

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
            IntervalleAcc�l�ration = 1f / 5f;
            Direction = new Vector3(-1, 0, 0);
            Vitesse = 0;
            base.Initialize();
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
            if (Temps�coul�DepuisMAJ >= IntervalleMAJ)
            {
                VarierVitesse();
                CalculerVitesse();
                AjusterPosition();
                EffectuerTransformations();
                //Recr�erMonde();
                Game.Window.Title = Vitesse.ToString("0.000");

                Temps�coul�DepuisMAJ = 0;
            }

            base.Update(gameTime);
        }


        void VarierVitesse()
        {
            bool acc�l�ration = GestionInput.EstEnfonc�e(Keys.W);
            bool freinage = GestionInput.EstEnfonc�e(Keys.S);
            int signe = Math.Sign(Vitesse) == 0 ? 1 : Math.Sign(Vitesse);


            if ((!acc�l�ration && !freinage)) { TempsAcc�l�ration += (float)-signe / 2.0f * IntervalleAcc�l�ration; }
            if (acc�l�ration) { TempsAcc�l�ration += 2f * IntervalleAcc�l�ration; DirectionModifi�e = true; }
            if (freinage) { TempsAcc�l�ration -= 3f * IntervalleAcc�l�ration; }

        }
        void CalculerVitesse()
        {
            Vitesse = FACTEUR_ACC�L�RATION * TempsAcc�l�ration;
        }
        int G�rerTouche(Keys touche)
        {
            return GestionInput.EstEnfonc�e(touche) ? INCR�MENT_ANGLE : 0;
        }


        void AjusterPosition()
        {
            Direction = Vitesse * Vector3.Normalize(new Vector3(-(float)Math.Sin(Rotation.Y), 0, -(float)Math.Cos(Rotation.Y))) / 100f;
            //p�dales + ajouter acc�l�ration??
            if(!GestionInput.EstEnfonc�e(Keys.LeftShift))
            {
                Position += Direction;
                Premi�reBoucleD�rapage = true;
            }
            else
            {
                if(Premi�reBoucleD�rapage)
                {
                    DirectionD�rapage = Direction;
                    Premi�reBoucleD�rapage = false;
                }
                else
                {
                     
                    Position += Vitesse * Vector3.Normalize(Direction + DirectionD�rapage);
                }
            }
           
            
            ChangementEffectu� = true;

            //Volant... degr�s??
            if (GestionInput.EstEnfonc�e(Keys.A) || GestionInput.EstEnfonc�e(Keys.D))
            {
                int sens = G�rerTouche(Keys.A) - G�rerTouche(Keys.D);

                //Rotation = new Vector3(Rotation.X, Rotation.Y + sens * INCR�MENT_ROTATION, Rotation.Z);
                Rotation = new Vector3(Rotation.X, Rotation.Y + sens * INCR�MENT_ROTATION * Vitesse / 60f, Rotation.Z);
                ChangementEffectu� = true;

            }
        }

        private void D�placerCam�ra()
        {
            if (DirectionModifi�e)
            {
                if (Vitesse > 30)
                {
                    AncienneDirection = new Vector3(Direction.X, Direction.Y, Direction.Z);
                }

                CalculerPositionCam�ra(Direction);
                Cam�ra.D�placer(PositionCam�ra, Position, Vector3.Up);
                DirectionModifi�e = false;
            }
            else
            {
                CalculerPositionCam�ra(AncienneDirection);
                Cam�ra.D�placer(PositionCam�ra, Position, Vector3.Up);
            }

        }

        void CalculerPositionCam�ra(Vector3 direction)
        {
            if (Vitesse > 25)
            {
                PositionCam�ra = Position - direction * Vitesse * 2 + new Vector3(0, 20, 0);
            }
            else
            {
                PositionCam�ra = Position - direction * 25 * 2 + new Vector3(0, 20, 0);
            }

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
        }
        public void Recr�erMonde()
        {
            Monde = Matrix.Identity;
            Monde *= Matrix.CreateScale(�chelle);
            Monde *= Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            Monde *= Matrix.CreateTranslation(Position);
        }

    }
}
