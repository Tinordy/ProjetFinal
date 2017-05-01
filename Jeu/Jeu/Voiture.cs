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
        const float COEFFICIENT_FROTTEMENT_GOMME_PNEU_GAZON = 0.5f;
        const int TEMPS_ACC�L�RATION_MIN = -5;
        const int VITESSE_MAX = 125;
        const int VITESSE_MIN = -50;
        const int TEMPS_ACC�L�RATION_MAX = 50;
        const float INCR�MENT_ROTATION = (float)Math.PI / 1080;
        const float COEFFICIENT_FROTTEMENT_GOMME_PNEU_ASPHALTE = 0.8f;
        const float INTERVALLE_RALENTISSEMENT = 1f / 6f;
        const int DISTANCE_CAM�RA = 400;
        const float FREINAGE = 1.5f;
        const float MAX_AXE = 65535f;
        const float MOITI�_AXE = 32767f;

        // propri�t�s
        Vector2 �tendueTotale { get; set; }
        const float FACTEUR_ACC�L�RATION = 1f / 5f;
        const int INCR�MENT_ANGLE = 10;
        const float RAYON_VOITURE = 1.5f;
        bool EstEnCollisionAvecOBJ { get; set; }
        protected float IntervalleMAJ { get; private set; }
        Vector3 PositionCam�ra { get; set; }
        Vector3 PositionAvant { get; set; }
        Vector3 PositionArri�re { get; set; }
        Vector3 DirectionCam�ra { get; set; }
        List<Vector2> PointsCentraux { get; set; }
        float NormeDirection { get; set; }
        float intervalleAcc�l�ration;
        Volant elVolant { get; set; }
        int RotationVolant { get; set; }
        int Acc�l�rationVolant { get; set; }
        int FreinageVolant { get; set; }
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
        DataPiste DataPiste { get; set; }
        Vector3 DirectionD�rapage { get; set; }
        bool Premi�reBoucleD�rapage { get; set; }
        public bool EstActif { get; set; }
        public BoundingSphere Sph�reDeCollisionAvant { get; set; }
        public BoundingSphere Sph�reDeCollisionArri�re { get; set; }
        public BoundingSphere Sph�reDeCollision { get; set; }

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
                if (value < TEMPS_ACC�L�RATION_MIN) { tempsAcc�l�ration = TEMPS_ACC�L�RATION_MIN; }
                if (value > TEMPS_ACC�L�RATION_MAX) { tempsAcc�l�ration = TEMPS_ACC�L�RATION_MAX; }
            }
        }

        float vitesse;
        float tempsAcc�l�ration;

        public Vector3 Direction { get; private set; }
        bool ChangementEffectu� { get; set; }
        Cam�ra Cam�ra { get; set; }

        InputManager GestionInput { get; set; }
        float IntervalleRotation
        {
            get
            {
                if (Vitesse <= -10)
                {
                    return 7f / 6f;
                }
                if (Vitesse <= -2)
                {
                    return 5f / 6f;
                }
                if (Vitesse <= 2)
                {
                    return 0;
                }
                if (Vitesse < 25)
                {
                    return 5f / 6f;
                }
                if (Vitesse < 50)
                {
                    return 6f / 6f;
                }
                if (Vitesse < 100)
                {
                    return 7f / 6f;
                }
                return 8f / 6f;
            }
        }

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
            elVolant = new Volant(Game, 1f / 60f);
            Game.Components.Add(elVolant);
            base.Initialize();
            PositionAvant = Position + Vector3.Normalize(Direction);
            PositionArri�re = Position - Vector3.Normalize(Direction);
            Sph�reDeCollisionAvant = new BoundingSphere(PositionAvant, RAYON_VOITURE);
            Sph�reDeCollisionArri�re = new BoundingSphere(PositionArri�re, RAYON_VOITURE);
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
            DataPiste = Game.Services.GetService(typeof(DataPiste)) as DataPiste;
            PointsCentraux = DataPiste.GetPointsCentraux();
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
                if (!elVolant.Enabled)
                {
                    VarierVitesseClavier();
                    CalculerVitesse();
                    AjusterPositionClavier();
                }
                else
                {
                    VarierVitesseVolant();
                    CalculerVitesse();
                    AjusterPositionVolant();
                    Game.Window.Title = "Vitesse : " + Vitesse.ToString() + " TempsAcc�l�ration : " + TempsAcc�l�ration.ToString() + " Axes : " + elVolant.AxeX.ToString() + " / " + elVolant.AxeY.ToString() + " / " + elVolant.AxeZ.ToString(); ;

                }
                EffectuerTransformations();
                //Recr�erMonde();
                //Game.Window.Title = "Position : " + Position.X.ToString("0.0") + " / " + Position.Y.ToString("0.0") + " / " + Position.Z.ToString("0.0") + " Vitesse : " + Vitesse.ToString("0.0") + " / TempsAcc�laration" + TempsAcc�l�ration.ToString("0.0");
                //Game.Window.Title = "Vitesse : " + Vitesse.ToString() + " TempsAcc�l�ration : " + TempsAcc�l�ration.ToString() + " Axes : " + elVolant.AxeX.ToString() + " / " + elVolant.AxeY.ToString() + " / " + elVolant.AxeZ.ToString(); ;
                Sph�reDeCollisionAvant = new BoundingSphere(PositionAvant, RAYON_VOITURE);
                Sph�reDeCollisionArri�re = new BoundingSphere(PositionArri�re, RAYON_VOITURE);
                Temps�coul�DepuisMAJ = 0;
            }

            base.Update(gameTime);
        }
        void VarierVitesseVolant()
        {
            int signe = Math.Sign(Vitesse) == 0 ? 1 : Math.Sign(Vitesse);
            TempsAcc�l�ration += INTERVALLE_RALENTISSEMENT * -(elVolant.AxeY - MOITI�_AXE) / MOITI�_AXE;
            TempsAcc�l�ration -= INTERVALLE_RALENTISSEMENT * FREINAGE * 3 * -((elVolant.AxeZ - MAX_AXE) / MAX_AXE);
            if (elVolant.AxeY == MOITI�_AXE && elVolant.AxeZ == MAX_AXE)
            {
                if (Vitesse >= -1f / 2f && Vitesse <= 1f / 2f)
                {
                    Vitesse = 0;
                    TempsAcc�l�ration = 0;
                }
                else
                {
                    TempsAcc�l�ration += (float)-signe * INTERVALLE_RALENTISSEMENT;
                }
            }


        }

        void AjusterPositionVolant()
        {
            Direction = Vitesse * Vector3.Normalize(new Vector3(-(float)Math.Sin(Rotation.Y), 0, -(float)Math.Cos(Rotation.Y))) / 100f;
            if (!elVolant.BoutonD�rapageActiv�)
            {
                ModifierPosition1();

                Premi�reBoucleD�rapage = true;
            }
            else
            {
                G�rerD�rapage();
            }

            ChangementEffectu� = true;

            if (!elVolant.BoutonD�rapageActiv�)
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
            float sens = ((float)elVolant.AxeX - MOITI�_AXE) / MOITI�_AXE;
            if (Vitesse > 0)
            {
                Rotation = new Vector3(Rotation.X, Rotation.Y - sens * INCR�MENT_ROTATION * IntervalleRotation * 12, Rotation.Z);

            }
            else
            {
                Rotation = new Vector3(Rotation.X, Rotation.Y + sens * INCR�MENT_ROTATION * IntervalleRotation * 12, Rotation.Z);
            }
            ChangementEffectu� = true;


        }

        void VarierVitesseClavier()
        {
            bool acc�l�ration = GestionInput.EstEnfonc�e(Keys.W);
            bool freinage = GestionInput.EstEnfonc�e(Keys.S);
            int signe = Math.Sign(Vitesse) == 0 ? 1 : Math.Sign(Vitesse);



            if ((!acc�l�ration && !freinage)) { TempsAcc�l�ration += (float)-signe * INTERVALLE_RALENTISSEMENT; }
            if (acc�l�ration && !GestionInput.EstEnfonc�e(Keys.LeftControl)) { TempsAcc�l�ration += 0.5f * FACTEUR_ACC�L�RATION; }
            if (freinage) { TempsAcc�l�ration -= FREINAGE * INTERVALLE_RALENTISSEMENT; }

        }

        void CalculerVitesse()
        {
            Vitesse = IntervalleAcc�l�ration * TempsAcc�l�ration;
        }
        int G�rerTouche(Keys touche)
        {
            return GestionInput.EstEnfonc�e(touche) ? INCR�MENT_ANGLE : 0;
        }

        void ModifierPosition1()
        {
            float x = IsOnTrack();
            Position += Direction * x;
            PositionAvant += Direction * x;
            PositionArri�re += Direction * x;
        }
        void ModifierPosition2()
        {
            Position -= Direction;
            PositionAvant -= Direction;
            PositionArri�re -= Direction;
        }
        void AjusterPositionClavier()
        {
            Direction = Vitesse * Vector3.Normalize(new Vector3(-(float)Math.Sin(Rotation.Y), 0, -(float)Math.Cos(Rotation.Y))) / 100f;
            //p�dales + ajouter acc�l�ration??
            if (!GestionInput.EstEnfonc�e(Keys.LeftControl))
            {
                ModifierPosition1();
                Premi�reBoucleD�rapage = true;
            }
            else
            {
                G�rerD�rapage();
            }

            ChangementEffectu� = true;

            //Volant... degr�s??
            if (GestionInput.EstEnfonc�e(Keys.A) || GestionInput.EstEnfonc�e(Keys.D))
            {
                int sens = G�rerTouche(Keys.A) - G�rerTouche(Keys.D);
                if (!GestionInput.EstEnfonc�e(Keys.LeftControl))
                {
                    if (TempsAcc�l�ration > 0)
                    {
                        TempsAcc�l�ration -= INTERVALLE_RALENTISSEMENT / 10;
                    }
                }
                //Rotation = new Vector3(Rotation.X, Rotation.Y + sens * INCR�MENT_ROTATION, Rotation.Z);
                Rotation = new Vector3(Rotation.X, Rotation.Y + sens * INCR�MENT_ROTATION * IntervalleRotation, Rotation.Z);
                ChangementEffectu� = true;
            }
        }
        void G�rerD�rapage()
        {
            if (Premi�reBoucleD�rapage && Vitesse > 5)
            {
                DirectionD�rapage = new Vector3(Direction.X, Direction.Y, Direction.Z);
                Premi�reBoucleD�rapage = false;
            }
            else if (Vitesse > 5)
            {
                DirectionD�rapage = Vitesse * Vector3.Normalize(DirectionD�rapage) / 100f;
                Direction = Vitesse * Vector3.Normalize(Direction) / 100f;
                float x = IsOnTrack();
                if (!EstEnCollisionAvecOBJ)
                {
                    Position += x * (Direction + DirectionD�rapage) / 2;
                    PositionArri�re += x * (Direction + DirectionD�rapage) / 2;
                    PositionAvant += x * (Direction + DirectionD�rapage) / 2;
                }
                else
                {
                    Position -= x * (Direction + DirectionD�rapage) / 2;
                    PositionArri�re -= x * (Direction + DirectionD�rapage) / 2;
                    PositionAvant -= x * (Direction + DirectionD�rapage) / 2;
                }

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
                    ModifierPosition1();
                }
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
            PositionCam�ra = Position - (DirectionCam�ra) * DISTANCE_CAM�RA + new Vector3(0, DISTANCE_CAM�RA / 2 * �chelle, 0);
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

        public bool EstEnCollision(object autreObjet)
        {
            bool valeurRetour1 = false;
            bool valeurRetour2 = false;
            if (autreObjet is ICollisionable)
            {
                valeurRetour1 = Sph�reDeCollisionAvant.Intersects((autreObjet as ICollisionable).Sph�reDeCollision);
                valeurRetour2 = Sph�reDeCollisionArri�re.Intersects((autreObjet as ICollisionable).Sph�reDeCollision);
            }
            EstEnCollisionAvecOBJ = (valeurRetour1 || valeurRetour2);
            return (valeurRetour1 || valeurRetour2);
        }
        public bool EstEnCollision2(Voiture ennemi)
        {
            bool valeurRetour1 = Sph�reDeCollisionArri�re.Intersects(ennemi.Sph�reDeCollisionArri�re);
            bool valeurRetour2 = Sph�reDeCollisionArri�re.Intersects(ennemi.Sph�reDeCollisionAvant);
            bool valeurRetour3 = Sph�reDeCollisionAvant.Intersects(ennemi.Sph�reDeCollisionArri�re);
            bool valeurRetour4 = Sph�reDeCollisionAvant.Intersects(ennemi.Sph�reDeCollisionAvant);

            EstEnCollisionAvecOBJ = (valeurRetour1 || valeurRetour2 || valeurRetour3 || valeurRetour4);
            return (valeurRetour1 || valeurRetour2 || valeurRetour3 || valeurRetour4);
        }
        public void Rebondir(Vector3 directionEnnemi, Vector3 centre)
        {
            if (Vitesse >= 1 || Vitesse <= -1)
            {
                //if (directionEnnemi == Vector3.Zero)
                //{
                    Vector3 collision = centre - Position;
                    double angleRad = Math.Acos(Vector3.Dot(collision, Direction) / Norme(collision, Vector3.Zero) / Norme(Direction, Vector3.Zero));
                    if (angleRad <= Math.PI / 5 || angleRad >= Math.PI * 4 / 5)
                    {
                        TempsAcc�l�ration = -TempsAcc�l�ration;
                        Direction = -Direction;
                        CalculerVitesse();
                        ModifierPosition1();
                    }
                    else
                    {
                        Rotation = new Vector3(Rotation.X, Rotation.Y - (float)angleRad * INCR�MENT_ROTATION * 2, Rotation.Z);
                    }
                    ChangementEffectu� = true;

                //}
                //else
                //{
                //    Vector3 newDirection = (Direction + directionEnnemi) / 2f;
                //    Direction = newDirection;
                //    Position += Direction / 100f;
                //    ChangementEffectu� = true;
                //}
            }
        }



        float IsOnTrack()
        {
            float temp = COEFFICIENT_FROTTEMENT_GOMME_PNEU_GAZON;
            foreach (Vector2 p in PointsCentraux)
            {
                if (Norme(new Vector3(p.X, 0, p.Y), Position) <= 7)
                {
                    temp = COEFFICIENT_FROTTEMENT_GOMME_PNEU_ASPHALTE;
                }
            }
            return temp;
        }
    }
}


