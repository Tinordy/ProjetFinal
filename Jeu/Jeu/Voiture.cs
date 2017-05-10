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
        const int VITESSE_MIN_POUR_DÉRAPER = 5;
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
        const int TEMPS_ACCÉLÉRATION_MIN = -5;
        const int VITESSE_MAX = 125;
        const int VITESSE_MIN = -50;
        const int TEMPS_ACCÉLÉRATION_MAX = 50;
        const float INCRÉMENT_ROTATION = (float)Math.PI / 1080;
        const float COEFFICIENT_FROTTEMENT_GOMME_PNEU_ASPHALTE = 0.8f;
        const float INTERVALLE_RALENTISSEMENT = 1f / 6f;
        const float INTERVALLE_ACCÉLÉRATION_INITIALE = 1f / 10f;
        const int DISTANCE_CAMÉRA = 400;
        const float FREINAGE = 1.5f;
        const float MAX_AXE = 65535f;
        const float MOITIÉ_AXE = 32767f;
        const int FACTEUR_FREIN_VOLANT = 3;
        const int FACTEUR_ROT_VOLANT = 12;
        const int LARGEUR_PISTE = 7;       
        const float FACTEUR_ACCÉLÉRATION = 1f / 3f;
        const int INCRÉMENT_ANGLE = 10;
        const float RAYON_VOITURE = 1.5f;

        // propriétés
        bool EstEnCollisionAvecOBJ { get; set; }
        protected float IntervalleMAJ { get; private set; }
        Vector3 PositionCaméra { get; set; }
        Vector3 nouvellePosition { get; set; }
        Vector3 PositionAvant { get; set; }
        Vector3 PositionArrière { get; set; }
        Vector3 DirectionCaméra { get; set; }
        List<Vector2> PointsCentraux { get; set; }
        Volant elVolant { get; set; }
        int AccélérationVolant { get; set; }

        #region FacteursPourVolantEtClavier
        float Sens
        {
            get { return Vitesse > 0 ? elVolant.Enabled ? -FacteurRotationVolant : FacteurRotationClavier : elVolant.Enabled ? FacteurRotationVolant : -FacteurRotationClavier; }
        }
        float FacteurAccélérationVolant
        {
            get { return -(elVolant.AxeY - MOITIÉ_AXE) / MOITIÉ_AXE; }
        }
        float FacteurFreinageVolant
        {
            get { return FACTEUR_FREIN_VOLANT * (-((elVolant.AxeZ - MAX_AXE) / MAX_AXE)); }
        }

        float FacteurRotationVolant
        {
            get { return FACTEUR_ROT_VOLANT * ((elVolant.AxeX - MOITIÉ_AXE) / MOITIÉ_AXE); }
        }

        float FacteurAccélérationClavier
        {
            get { return GestionInput.EstEnfoncée(Keys.W) ? 1 : 0; }
        }
        float FacteurFreinageClavier
        {
            get { return GestionInput.EstEnfoncée(Keys.S) ? 1 : 0; }
        }

        float FacteurRotationClavier
        {
            get { return GérerTouche(Keys.A) - GérerTouche(Keys.D); }
        }
        float FacteurAccélérationFinal
        {
            get { return elVolant.Enabled ? FacteurAccélérationVolant : FacteurAccélérationClavier; }
        }
        float FacteurFreinageFinal
        {
            get { return elVolant.Enabled ? FacteurFreinageVolant : FacteurFreinageClavier; }
        }
        float FacteurRotationFinal
        {
            get { return elVolant.Enabled ? FacteurRotationVolant : FacteurRotationClavier; }
        }
        bool BoutonDérapageActivé
        {
            get { return elVolant.Enabled ? elVolant.BoutonDérapageActivé : GestionInput.EstEnfoncée(Keys.LeftControl); }
        }
        #endregion

        float intervalleAccélération;
        float IntervalleAccélération
        {
            get
            {
                if (TempsAccélération < INTERVALLE_ACC_1)
                {
                    return ACC_1;
                }
                else if (TempsAccélération < INTERVALLE_ACC_2)
                {
                    return ACC_2;
                }
                else if (TempsAccélération >= INTERVALLE_ACC_2)
                {
                    return ACC_3;
                }
                return intervalleAccélération;

            }
            set { intervalleAccélération = value; }
        }
        float TempsÉcouléDepuisMAJ { get; set; }
        Réseautique GérerRéseau { get; set; }
        DataPiste DataPiste { get; set; }
        Vector3 DirectionDérapage { get; set; }
        bool PremièreBoucleDérapage { get; set; }
        public bool EstActif { get;  set; }
        public BoundingSphere SphèreDeCollisionAvant { get; protected set; }
        public BoundingSphere SphèreDeCollisionArrière { get; protected set; }
        // pour interface
        public BoundingSphere SphèreDeCollision { get; protected set; }

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
        public float TempsAccélération
        {
            get { return tempsAccélération; }
            private set
            {
                tempsAccélération = value;
                if (value < TEMPS_ACCÉLÉRATION_MIN) { tempsAccélération = TEMPS_ACCÉLÉRATION_MIN; }
                else if (value > TEMPS_ACCÉLÉRATION_MAX) { tempsAccélération = TEMPS_ACCÉLÉRATION_MAX; }
            }
        }

        float vitesse;
        float tempsAccélération;

        public Vector3 Direction { get; private set; }
        bool ChangementEffectué { get; set; }
        Caméra Caméra { get; set; }
        Vector3 Décalage { get; set; }
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
            IntervalleAccélération = INTERVALLE_ACCÉLÉRATION_INITIALE;
            Direction = Vector3.Normalize(new Vector3(0, (float)Math.PI, 0));
            Vitesse = 0;
            elVolant = new Volant(Game, IntervalleMAJ);
            Game.Components.Add(elVolant);
            base.Initialize();
            Décalage = Vector3.Normalize(Direction);
            PositionAvant = Position + Vector3.Normalize(Direction);
            PositionArrière = Position - Vector3.Normalize(Direction);
            SphèreDeCollisionAvant = new BoundingSphere(PositionAvant, RAYON_VOITURE);
            SphèreDeCollisionArrière = new BoundingSphere(PositionArrière, RAYON_VOITURE);
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
            float tempsÉcoulé = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TempsÉcouléDepuisMAJ += tempsÉcoulé;
            if (TempsÉcouléDepuisMAJ >= IntervalleMAJ && EstActif)
            {
                VarierVitesse();
                CalculerVitesse();
                AjusterPosition();
                EffectuerTransformations();                
                TempsÉcouléDepuisMAJ = 0;
            }

            base.Update(gameTime);
        }

        void VarierVitesse()
        {
            int signe = Math.Sign(Vitesse) == 0 ? 1 : Math.Sign(Vitesse);
            if (!BoutonDérapageActivé) { TempsAccélération += FACTEUR_ACCÉLÉRATION * FacteurAccélérationFinal; }
            TempsAccélération -= INTERVALLE_RALENTISSEMENT * FREINAGE * FacteurFreinageFinal;
            if (FacteurAccélérationFinal == 0 && FacteurFreinageFinal == 0)
            {
                // la vitesse oscille parfois alors il faut prévenir ça (dû à l'accélération qui peut dépasser)
                if (Vitesse >= -INCERTITUDE_VITESSE && Vitesse <= INCERTITUDE_VITESSE)
                {
                    Vitesse = 0;
                    TempsAccélération = 0;
                }
                else
                {
                    TempsAccélération += (float)-signe * INTERVALLE_RALENTISSEMENT;
                }
            }
        }

        void AjusterPosition()
        {
            Direction = Vitesse * Vector3.Normalize(new Vector3(-(float)Math.Sin(Rotation.Y), 0, -(float)Math.Cos(Rotation.Y))) / 100f;
            if (!BoutonDérapageActivé)
            {
                ModifierPosition();

                PremièreBoucleDérapage = true;
            }
            else
            {
                GérerDérapage();
            }

            ChangementEffectué = true;

            if (FacteurRotationFinal != 0)
            {
                if (TempsAccélération > 0)
                {
                    TempsAccélération -= INTERVALLE_RALENTISSEMENT / 10;
                }
            }

            Rotation = new Vector3(Rotation.X, Rotation.Y + Sens * INCRÉMENT_ROTATION * IntervalleRotation, Rotation.Z);

            ChangementEffectué = true;
        }


        void CalculerVitesse()
        {
            Vitesse = IntervalleAccélération * TempsAccélération;
        }
        int GérerTouche(Keys touche)
        {
            return GestionInput.EstEnfoncée(touche) ? INCRÉMENT_ANGLE : 0;
        }

        void ModifierPosition()
        {
            float x = IsOnTrack();
            nouvellePosition = Position + Direction * x;
            if (EstSurLeTerrain())
            {
                Position += Direction * x;
                PositionAvant += Direction * x;
                PositionArrière += Direction * x;
            }
        }
        bool EstSurLeTerrain()
        {
            return nouvellePosition.X < LARGEUR_TERRAIN && nouvellePosition.X > 0 && nouvellePosition.Z < LONGUEUR_TERRAIN && nouvellePosition.Z > 0;
        }

        void GérerDérapage()
        {
            // à la première boucle je détermine le vecteur intermédiaire qui me permettra de diriger le dérapage
            if (PremièreBoucleDérapage && Vitesse > 5)
            {
                DirectionDérapage = new Vector3(Direction.X, Direction.Y, Direction.Z);
                PremièreBoucleDérapage = false;
            }
            else if (Vitesse > 5)
            {
                DirectionDérapage = Vitesse * Vector3.Normalize(DirectionDérapage) / 100f;
                Direction = Vitesse * Vector3.Normalize(Direction) / 100f;
                // x étant la constante qui régit la vitesse de la voiture selon la surface sur laquelle 
                // elle se situe.
                float x = IsOnTrack();
                if (!EstEnCollisionAvecOBJ)
                {
                    // j'additione le vecteur intermédiaire et le vecteur direction pour trouver celui au milieu en
                    // divisant par deux pour trouver l'orientation de ma voiture lors du dérapage...
                    nouvellePosition = Position + x * (Direction + DirectionDérapage) / 2;
                    if (EstSurLeTerrain())
                    {
                        Position += x * (Direction + DirectionDérapage) / 2;
                        PositionArrière += x * (Direction + DirectionDérapage) / 2;
                        PositionAvant += x * (Direction + DirectionDérapage) / 2;
                    }
                }
                else // au cas où il y a collision en dérapant je veux lui donner un comprtement un peu différent qu,une collision normale 
                // en plus du bond qu'elle fait au début.
                {
                    nouvellePosition = Position - x * (Direction + DirectionDérapage) / 2;
                    if (EstSurLeTerrain())
                    {
                        Position -= x * (Direction + DirectionDérapage) / 2;
                        PositionArrière -= x * (Direction + DirectionDérapage) / 2;
                        PositionAvant -= x * (Direction + DirectionDérapage) / 2;
                    }
                }
                // Il faut gérer le temps d'accélération différement pour le ralentissement.
                // le premier if est nécessaire sinon on se retrouve avec des valeurs non numériques
                // à certaines occasions.
                if (TempsAccélération <= 0)
                {
                    TempsAccélération = 0;
                }
                else if (TempsAccélération > 0)
                {
                    TempsAccélération -= IntervalleMAJ * TempsAccélération;
                }
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
            // pour regarder en arrière
            if (GestionInput.EstEnfoncée(Keys.LeftAlt) || elVolant.Enabled && elVolant.BoutonCaméraArrièreActivé)
            {
                DirectionCaméra = -DirectionCaméra;
            }
            // je préfère une caméra qui suit tout le temps la voiture derrière.
            PositionCaméra = Position - (DirectionCaméra) * DISTANCE_CAMÉRA + new Vector3(0, DISTANCE_CAMÉRA / 2 * Échelle, 0);
        }

        float CalculerPosition(int déplacement, float posActuelle)
        {
            return posActuelle + déplacement;
        }

        private void EffectuerTransformations()
        {
            if (ChangementEffectué)
            {
                Monde = Matrix.CreateScale(Échelle) * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) * Matrix.CreateTranslation(Position);
                // même si on joue seul demande de Marilou
                GérerRéseau.SendMatriceMonde(Monde);
                SphèreDeCollisionAvant = new BoundingSphere(PositionAvant, RAYON_VOITURE);
                SphèreDeCollisionArrière = new BoundingSphere(PositionArrière, RAYON_VOITURE);                
                DéplacerCaméra();
                ChangementEffectué = false;
            }
        }
        public void AjusterPosition(Matrix nouvelleMatriceMonde)
        {
            Monde = nouvelleMatriceMonde;
            SphèreDeCollisionAvant = new BoundingSphere(Monde.Translation + Vector3.Normalize(Monde.Forward - Monde.Backward), RAYON_VOITURE);
            SphèreDeCollisionArrière = new BoundingSphere(Monde.Translation - Vector3.Normalize(Monde.Forward - Monde.Backward), RAYON_VOITURE);
        }

        // Pour les voitures robots
        public void RecréerMonde()
        {
            Monde = Matrix.Identity;
            Monde *= Matrix.CreateScale(Échelle);
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
                valeurRetour1 = SphèreDeCollisionAvant.Intersects((autreObjet as Maison).BoxDeCollision);
                valeurRetour2 = SphèreDeCollisionArrière.Intersects((autreObjet as Maison).BoxDeCollision);
            }
            // sphères
            else if (autreObjet is ICollisionable)
            {
                valeurRetour1 = SphèreDeCollisionAvant.Intersects((autreObjet as ICollisionable).SphèreDeCollision);
                valeurRetour2 = SphèreDeCollisionArrière.Intersects((autreObjet as ICollisionable).SphèreDeCollision);
            }
            EstEnCollisionAvecOBJ = (valeurRetour1 || valeurRetour2);
            return (valeurRetour1 || valeurRetour2);
        }
        // collision avec voiture
        public bool EstEnCollision2(Voiture ennemi)
        {
            // 2 x 2 sphères de collisions
            bool valeurRetour = false;
            if (SphèreDeCollisionArrière.Intersects(ennemi.SphèreDeCollisionArrière))
            {
                valeurRetour = true;
            }
            else if(SphèreDeCollisionArrière.Intersects(ennemi.SphèreDeCollisionAvant))
            {
                valeurRetour = true;
            }
            else if(SphèreDeCollisionAvant.Intersects(ennemi.SphèreDeCollisionArrière))
            {
                valeurRetour = true;
            }
            else if(SphèreDeCollisionAvant.Intersects(ennemi.SphèreDeCollisionAvant))
            {
                valeurRetour = true;
            }

            EstEnCollisionAvecOBJ = valeurRetour;
            return EstEnCollisionAvecOBJ;
        }
        public void Rebondir(Vector3 directionEnnemi, Vector3 centre)
        {
            // même chose que tantôt
            // je laisse les commentaires au cas où je voudrais retravailler dessus la classe 
            // dans un avenir pour faire que la voiture rebondisse avec un angle.
            // j'ai essayé mais j,ai abandonné puisque j'étais seulement capable de la faire rebondir 
            // d'un côté et j'ai manqué de temps.
            if (Vitesse >= INCERTITUDE_VITESSE || Vitesse <= -INCERTITUDE_VITESSE)
            {
                //if (directionEnnemi == Vector3.Zero)
                //{
                Vector3 collision = centre - Position;
                //double angleRad = Math.Acos(Vector3.Dot(collision, Direction) / Norme(collision, Vector3.Zero) / Norme(Direction, Vector3.Zero));
                //if (angleRad <= Math.PI / 5 || angleRad >= Math.PI * 4 / 5)
                //{
                TempsAccélération = -TempsAccélération;
                Direction = -Direction;
                CalculerVitesse();
           
                ModifierPosition(); // ----\
                                    //      > je veux qu'elle rebondisse assez loin alors on le fait 2 fois.
                ModifierPosition(); // ----/
                //}
                //else
                //{
                //    Rotation = new Vector3(Rotation.X, Rotation.Y - (float)angleRad * INCRÉMENT_ROTATION * 2, Rotation.Z);
                //}
                ChangementEffectué = true;

                //}
                //else
                //{
                //    Vector3 newDirection = (Direction + directionEnnemi) / 2f;
                //    Direction = newDirection;
                //    Position += Direction / 100f;
                //    ChangementEffectué = true;
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


