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
        const float ACC_1 = 6f;
        const float ACC_2 = 5f;
        const float ACC_3 = 4f;
        const int INTERVALLE_ACC_1 = 10;
        const int INTERVALLE_ACC_2 = 20;
        const int LARGEUR_TERRAIN = 500;
        const int LONGUEUR_TERRAIN = 350;
        const int VITESSE_MIN_POUR_D�RAPER = 5;
        const float ROTATION_1 = 7f / 6f;
        const float ROTATION_2 = 5f / 6f;
        const float ROTATION_3 = 0;
        const float ROTATION_4 = 6f / 6f;
        const float ROTATION_5 = 7f / 6f;
        const float ROTATION_6 = 8f / 6f;
        const float VITESSE_1 = -10;
        const float VITESSE_2 = -2;
        const float VITESSE_3 = 2;
        const float VITESSE_4 = 25;
        const float VITESSE_5 = 50;
        const float VITESSE_6 = 100;
        const float INCERTITUDE_VITESSE = 1f / 2f;
        const float COEFFICIENT_FROTTEMENT_GOMME_PNEU_GAZON = 0.5f;
        const int TEMPS_ACC�L�RATION_MIN = -5;
        const int VITESSE_MAX = 125;
        const int VITESSE_MIN = -50;
        const int TEMPS_ACC�L�RATION_MAX = 50;
        const float INCR�MENT_ROTATION = (float)Math.PI / 1080;
        const float COEFFICIENT_FROTTEMENT_GOMME_PNEU_ASPHALTE = 0.8f;
        const float INTERVALLE_RALENTISSEMENT = 1f / 6f;
        const float INTERVALLE_ACC�L�RATION_INITIALE = 1f / 10f;
        const int DISTANCE_CAM�RA = 400;
        const float FREINAGE = 1.5f;
        const float MAX_AXE = 65535f;
        const float MOITI�_AXE = 32767f;
        const int FACTEUR_FREIN_VOLANT = 3;
        const int FACTEUR_ROT_VOLANT = 12;
        const int LARGEUR_PISTE = 7;       
        const float FACTEUR_ACC�L�RATION = 1f / 3f;
        const int INCR�MENT_ANGLE = 10;
        const float RAYON_VOITURE = 1.5f;

        // propri�t�s
        bool EstEnCollisionAvecOBJ { get; set; }
        protected float IntervalleMAJ { get; private set; }
        Vector3 PositionCam�ra { get; set; }
        Vector3 nouvellePosition { get; set; }
        Vector3 PositionAvant { get; set; }
        Vector3 PositionArri�re { get; set; }
        Vector3 DirectionCam�ra { get; set; }
        List<Vector2> PointsCentraux { get; set; }
        Volant elVolant { get; set; }
        int Acc�l�rationVolant { get; set; }

        #region FacteursPourVolantEtClavier
        float Sens
        {
            get { return Vitesse > 0 ? elVolant.Enabled ? -FacteurRotationVolant : FacteurRotationClavier : elVolant.Enabled ? FacteurRotationVolant : -FacteurRotationClavier; }
        }
        float FacteurAcc�l�rationVolant
        {
            get { return -(elVolant.AxeY - MOITI�_AXE) / MOITI�_AXE; }
        }
        float FacteurFreinageVolant
        {
            get { return FACTEUR_FREIN_VOLANT * (-((elVolant.AxeZ - MAX_AXE) / MAX_AXE)); }
        }

        float FacteurRotationVolant
        {
            get { return FACTEUR_ROT_VOLANT * ((elVolant.AxeX - MOITI�_AXE) / MOITI�_AXE); }
        }

        float FacteurAcc�l�rationClavier
        {
            get { return GestionInput.EstEnfonc�e(Keys.W) ? 1 : 0; }
        }
        float FacteurFreinageClavier
        {
            get { return GestionInput.EstEnfonc�e(Keys.S) ? 1 : 0; }
        }

        float FacteurRotationClavier
        {
            get { return G�rerTouche(Keys.A) - G�rerTouche(Keys.D); }
        }
        float FacteurAcc�l�rationFinal
        {
            get { return elVolant.Enabled ? FacteurAcc�l�rationVolant : FacteurAcc�l�rationClavier; }
        }
        float FacteurFreinageFinal
        {
            get { return elVolant.Enabled ? FacteurFreinageVolant : FacteurFreinageClavier; }
        }
        float FacteurRotationFinal
        {
            get { return elVolant.Enabled ? FacteurRotationVolant : FacteurRotationClavier; }
        }
        bool BoutonD�rapageActiv�
        {
            get { return elVolant.Enabled ? elVolant.BoutonD�rapageActiv� : GestionInput.EstEnfonc�e(Keys.LeftControl); }
        }
        #endregion

        float intervalleAcc�l�ration;
        float IntervalleAcc�l�ration
        {
            get
            {
                if (TempsAcc�l�ration < INTERVALLE_ACC_1)
                {
                    return ACC_1;
                }
                else if (TempsAcc�l�ration < INTERVALLE_ACC_2)
                {
                    return ACC_2;
                }
                else if (TempsAcc�l�ration >= INTERVALLE_ACC_2)
                {
                    return ACC_3;
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
        public bool EstActif { get;  set; }
        public BoundingSphere Sph�reDeCollisionAvant { get; protected set; }
        public BoundingSphere Sph�reDeCollisionArri�re { get; protected set; }
        // pour interface
        public BoundingSphere Sph�reDeCollision { get; protected set; }

        public float Vitesse
        {
            get
            { return vitesse; }
            private set
            {
                vitesse = value;

                if (value < VITESSE_MIN) { vitesse = VITESSE_MIN; }
                else if (value > VITESSE_MAX) { vitesse = VITESSE_MAX; }

            }
        }
        public float TempsAcc�l�ration
        {
            get { return tempsAcc�l�ration; }
            private set
            {
                tempsAcc�l�ration = value;
                if (value < TEMPS_ACC�L�RATION_MIN) { tempsAcc�l�ration = TEMPS_ACC�L�RATION_MIN; }
                else if (value > TEMPS_ACC�L�RATION_MAX) { tempsAcc�l�ration = TEMPS_ACC�L�RATION_MAX; }
            }
        }

        float vitesse;
        float tempsAcc�l�ration;

        public Vector3 Direction { get; private set; }
        bool ChangementEffectu� { get; set; }
        Cam�ra Cam�ra { get; set; }
        Vector3 D�calage { get; set; }
        InputManager GestionInput { get; set; }
        float IntervalleRotation
        {

            get
            {
                if (Vitesse <= VITESSE_1)
                {
                    return ROTATION_1;
                }
                else if (Vitesse <= VITESSE_2)
                {
                    return ROTATION_2;
                }
                else if (Vitesse <= VITESSE_3)
                {
                    return ROTATION_3;
                }
                else if (Vitesse < VITESSE_4)
                {
                    return ROTATION_2;
                }
                else if (Vitesse < VITESSE_5)
                {
                    return ROTATION_4;
                }
                else if (Vitesse < VITESSE_6)
                {
                    return ROTATION_5;
                }
                return ROTATION_6;
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
            IntervalleAcc�l�ration = INTERVALLE_ACC�L�RATION_INITIALE;
            Direction = Vector3.Normalize(new Vector3(0, (float)Math.PI, 0));
            Vitesse = 0;
            elVolant = new Volant(Game, IntervalleMAJ);
            Game.Components.Add(elVolant);
            base.Initialize();
            D�calage = Vector3.Normalize(Direction);
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
                VarierVitesse();
                CalculerVitesse();
                AjusterPosition();
                EffectuerTransformations();                
                Temps�coul�DepuisMAJ = 0;
            }

            base.Update(gameTime);
        }

        void VarierVitesse()
        {
            int signe = Math.Sign(Vitesse) == 0 ? 1 : Math.Sign(Vitesse);
            if (!BoutonD�rapageActiv�) { TempsAcc�l�ration += FACTEUR_ACC�L�RATION * FacteurAcc�l�rationFinal; }
            TempsAcc�l�ration -= INTERVALLE_RALENTISSEMENT * FREINAGE * FacteurFreinageFinal;
            if (FacteurAcc�l�rationFinal == 0 && FacteurFreinageFinal == 0)
            {
                // la vitesse oscille parfois alors il faut pr�venir �a (d� � l'acc�l�ration qui peut d�passer)
                if (Vitesse >= -INCERTITUDE_VITESSE && Vitesse <= INCERTITUDE_VITESSE)
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

        void AjusterPosition()
        {
            Direction = Vitesse * Vector3.Normalize(new Vector3(-(float)Math.Sin(Rotation.Y), 0, -(float)Math.Cos(Rotation.Y))) / 100f;
            if (!BoutonD�rapageActiv�)
            {
                ModifierPosition();

                Premi�reBoucleD�rapage = true;
            }
            else
            {
                G�rerD�rapage();
            }

            ChangementEffectu� = true;

            if (FacteurRotationFinal != 0)
            {
                if (TempsAcc�l�ration > 0)
                {
                    TempsAcc�l�ration -= INTERVALLE_RALENTISSEMENT / 10;
                }
            }

            Rotation = new Vector3(Rotation.X, Rotation.Y + Sens * INCR�MENT_ROTATION * IntervalleRotation, Rotation.Z);

            ChangementEffectu� = true;
        }


        void CalculerVitesse()
        {
            Vitesse = IntervalleAcc�l�ration * TempsAcc�l�ration;
        }
        int G�rerTouche(Keys touche)
        {
            return GestionInput.EstEnfonc�e(touche) ? INCR�MENT_ANGLE : 0;
        }

        void ModifierPosition()
        {
            float x = IsOnTrack();
            nouvellePosition = Position + Direction * x;
            if (EstSurLeTerrain())
            {
                Position += Direction * x;
                PositionAvant += Direction * x;
                PositionArri�re += Direction * x;
            }
        }
        bool EstSurLeTerrain()
        {
            return nouvellePosition.X < LARGEUR_TERRAIN && nouvellePosition.X > 0 && nouvellePosition.Z < LONGUEUR_TERRAIN && nouvellePosition.Z > 0;
        }

        void G�rerD�rapage()
        {
            // � la premi�re boucle je d�termine le vecteur interm�diaire qui me permettra de diriger le d�rapage
            if (Premi�reBoucleD�rapage && Vitesse > 5)
            {
                DirectionD�rapage = new Vector3(Direction.X, Direction.Y, Direction.Z);
                Premi�reBoucleD�rapage = false;
            }
            else if (Vitesse > 5)
            {
                DirectionD�rapage = Vitesse * Vector3.Normalize(DirectionD�rapage) / 100f;
                Direction = Vitesse * Vector3.Normalize(Direction) / 100f;
                // x �tant la constante qui r�git la vitesse de la voiture selon la surface sur laquelle 
                // elle se situe.
                float x = IsOnTrack();
                if (!EstEnCollisionAvecOBJ)
                {
                    // j'additione le vecteur interm�diaire et le vecteur direction pour trouver celui au milieu en
                    // divisant par deux pour trouver l'orientation de ma voiture lors du d�rapage...
                    nouvellePosition = Position + x * (Direction + DirectionD�rapage) / 2;
                    if (EstSurLeTerrain())
                    {
                        Position += x * (Direction + DirectionD�rapage) / 2;
                        PositionArri�re += x * (Direction + DirectionD�rapage) / 2;
                        PositionAvant += x * (Direction + DirectionD�rapage) / 2;
                    }
                }
                else // au cas o� il y a collision en d�rapant je veux lui donner un comprtement un peu diff�rent qu,une collision normale 
                // en plus du bond qu'elle fait au d�but.
                {
                    nouvellePosition = Position - x * (Direction + DirectionD�rapage) / 2;
                    if (EstSurLeTerrain())
                    {
                        Position -= x * (Direction + DirectionD�rapage) / 2;
                        PositionArri�re -= x * (Direction + DirectionD�rapage) / 2;
                        PositionAvant -= x * (Direction + DirectionD�rapage) / 2;
                    }
                }
                // Il faut g�rer le temps d'acc�l�ration diff�rement pour le ralentissement.
                // le premier if est n�cessaire sinon on se retrouve avec des valeurs non num�riques
                // � certaines occasions.
                if (TempsAcc�l�ration <= 0)
                {
                    TempsAcc�l�ration = 0;
                }
                else if (TempsAcc�l�ration > 0)
                {
                    TempsAcc�l�ration -= IntervalleMAJ * TempsAcc�l�ration;
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
            // pour regarder en arri�re
            if (GestionInput.EstEnfonc�e(Keys.LeftAlt) || elVolant.Enabled && elVolant.BoutonCam�raArri�reActiv�)
            {
                DirectionCam�ra = -DirectionCam�ra;
            }
            // je pr�f�re une cam�ra qui suit tout le temps la voiture derri�re.
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
                Monde = Matrix.CreateScale(�chelle) * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) * Matrix.CreateTranslation(Position);
                // m�me si on joue seul demande de Marilou
                G�rerR�seau.SendMatriceMonde(Monde);
                Sph�reDeCollisionAvant = new BoundingSphere(PositionAvant, RAYON_VOITURE);
                Sph�reDeCollisionArri�re = new BoundingSphere(PositionArri�re, RAYON_VOITURE);                
                D�placerCam�ra();
                ChangementEffectu� = false;
            }
        }
        public void AjusterPosition(Matrix nouvelleMatriceMonde)
        {
            Monde = nouvelleMatriceMonde;
            Sph�reDeCollisionAvant = new BoundingSphere(Monde.Translation + Vector3.Normalize(Monde.Forward - Monde.Backward), RAYON_VOITURE);
            Sph�reDeCollisionArri�re = new BoundingSphere(Monde.Translation - Vector3.Normalize(Monde.Forward - Monde.Backward), RAYON_VOITURE);
        }

        // Pour les voitures robots
        public void Recr�erMonde()
        {
            Monde = Matrix.Identity;
            Monde *= Matrix.CreateScale(�chelle);
            Monde *= Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            Monde *= Matrix.CreateTranslation(Position);
        }
        //collisions avec objets
        public bool EstEnCollision(object autreObjet)
        {
            bool valeurRetour1 = false;
            bool valeurRetour2 = false;
            // box
            if (autreObjet is Maison)
            {
                valeurRetour1 = Sph�reDeCollisionAvant.Intersects((autreObjet as Maison).BoxDeCollision);
                valeurRetour2 = Sph�reDeCollisionArri�re.Intersects((autreObjet as Maison).BoxDeCollision);
            }
            // sph�res
            else if (autreObjet is ICollisionable)
            {
                valeurRetour1 = Sph�reDeCollisionAvant.Intersects((autreObjet as ICollisionable).Sph�reDeCollision);
                valeurRetour2 = Sph�reDeCollisionArri�re.Intersects((autreObjet as ICollisionable).Sph�reDeCollision);
            }
            EstEnCollisionAvecOBJ = (valeurRetour1 || valeurRetour2);
            return (valeurRetour1 || valeurRetour2);
        }
        // collision avec voiture
        public bool EstEnCollision2(Voiture ennemi)
        {
            // 2 x 2 sph�res de collisions
            bool valeurRetour = false;
            if (Sph�reDeCollisionArri�re.Intersects(ennemi.Sph�reDeCollisionArri�re))
            {
                valeurRetour = true;
            }
            else if(Sph�reDeCollisionArri�re.Intersects(ennemi.Sph�reDeCollisionAvant))
            {
                valeurRetour = true;
            }
            else if(Sph�reDeCollisionAvant.Intersects(ennemi.Sph�reDeCollisionArri�re))
            {
                valeurRetour = true;
            }
            else if(Sph�reDeCollisionAvant.Intersects(ennemi.Sph�reDeCollisionAvant))
            {
                valeurRetour = true;
            }

            EstEnCollisionAvecOBJ = valeurRetour;
            return EstEnCollisionAvecOBJ;
        }
        public void Rebondir(Vector3 directionEnnemi, Vector3 centre)
        {
            // m�me chose que tant�t
            // je laisse les commentaires au cas o� je voudrais retravailler dessus la classe 
            // dans un avenir pour faire que la voiture rebondisse avec un angle.
            // j'ai essay� mais j,ai abandonn� puisque j'�tais seulement capable de la faire rebondir 
            // d'un c�t� et j'ai manqu� de temps.
            if (Vitesse >= INCERTITUDE_VITESSE || Vitesse <= -INCERTITUDE_VITESSE)
            {
                //if (directionEnnemi == Vector3.Zero)
                //{
                Vector3 collision = centre - Position;
                //double angleRad = Math.Acos(Vector3.Dot(collision, Direction) / Norme(collision, Vector3.Zero) / Norme(Direction, Vector3.Zero));
                //if (angleRad <= Math.PI / 5 || angleRad >= Math.PI * 4 / 5)
                //{
                TempsAcc�l�ration = -TempsAcc�l�ration;
                Direction = -Direction;
                CalculerVitesse();
           
                ModifierPosition(); // ----\
                                    //      > je veux qu'elle rebondisse assez loin alors on le fait 2 fois.
                ModifierPosition(); // ----/
                //}
                //else
                //{
                //    Rotation = new Vector3(Rotation.X, Rotation.Y - (float)angleRad * INCR�MENT_ROTATION * 2, Rotation.Z);
                //}
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
                if (Norme(new Vector3(p.X, 0, p.Y), Position) <= LARGEUR_PISTE)
                {
                    temp = COEFFICIENT_FROTTEMENT_GOMME_PNEU_ASPHALTE;
                }
            }
            return temp;
        }
    }
}


