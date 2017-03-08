using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace AtelierXNA
{
    public delegate void FonctionÉvénementielle();

    public class BoutonDeCommande : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Color COULEUR_PAR_DÉFAUT = Color.Black;
        Color COULEUR_FOCUS = Color.White;
        Color COULEUR_INACTIF = Color.Gray;
        string Texte { get; set; }
        string NomFont { get; set; }
        string NomImageNormale { get; set; }
        string NomImageEnfoncée { get; set; }
        Vector2 Position { get; set; }
        Vector2 PositionChaîne { get; set; }
        Vector2 OrigineChaîne { get; set; }
        SpriteFont PoliceDeCaractères { get; set; }
        Texture2D ImageNormale { get; set; }
        Texture2D ImageEnfoncée { get; set; }
        Texture2D ImageBouton { get; set; }
        Rectangle RectangleDestination { get; set; }
        Color CouleurTexte { get; set; }
        SpriteBatch GestionSprites { get; set; }
        InputManager GestionInput { get; set; }
        float IntervalleMAJ { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }

        FonctionÉvénementielle OnClick { get; set; }

        bool estActif;

        public bool EstActif
        {
            get { return estActif; }
            set
            {
                estActif = value;
                CouleurTexte = estActif ? COULEUR_PAR_DÉFAUT : COULEUR_INACTIF;
            }
        }

        public BoutonDeCommande(Game jeu, string texte, string nomFont,
                                string nomImageNormale, string nomImageEnfoncée,
                                Vector2 position, bool estActif,
                                FonctionÉvénementielle onClick, float intervalleMAJ)
           : base(jeu)
        {
            Texte = texte;
            NomFont = nomFont;
            NomImageNormale = nomImageNormale;
            NomImageEnfoncée = nomImageEnfoncée;
            Position = position;
            EstActif = estActif;
            OnClick = onClick;
            IntervalleMAJ = intervalleMAJ;
        }

        public override void Initialize()
        {
            TempsÉcouléDepuisMAJ = 0;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Vector2 dimensionChaîne;
            Vector2 dimension;
            base.LoadContent();

            GestionSprites = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;

            PoliceDeCaractères = Game.Content.Load<SpriteFont>("Fonts/" + NomFont);
            ImageNormale = Game.Content.Load<Texture2D>("Textures/" + NomImageNormale);
            ImageEnfoncée = Game.Content.Load<Texture2D>("Textures/" + NomImageEnfoncée);

            ImageBouton = ImageNormale;

            dimensionChaîne = PoliceDeCaractères.MeasureString(Texte);
            dimension = dimensionChaîne * 1.10f;
            Position = Position - dimension / 2;
            PositionChaîne = new Vector2(Position.X + dimension.X / 2, Position.Y + dimension.Y / 2);
            OrigineChaîne = new Vector2(dimensionChaîne.X / 2, dimensionChaîne.Y / 2);
            RectangleDestination = new Rectangle((int)Position.X, (int)Position.Y, (int)dimension.X, (int)dimension.Y);
        }

        public override void Update(GameTime gameTime)
        {
            if (EstActif)
            {
                Point positionSouris = GestionInput.GetPositionSouris();
                if (RectangleDestination.Contains(positionSouris))
                {
                    CouleurTexte = COULEUR_FOCUS;
                    if (GestionInput.EstNouveauClicGauche())
                    {
                        OnClick();
                        ImageBouton = ImageEnfoncée;
                    }
                    else
                    {
                        if (!GestionInput.EstAncienClicGauche())
                        {
                            if (GestionInput.EstAncienClicDroit())
                            {
                                MiseÀJour(gameTime);
                                ImageBouton = ImageEnfoncée;
                            }
                            else
                            {
                                ImageBouton = ImageNormale;
                            }
                        }
                    }
                }
                else
                {
                    CouleurTexte = COULEUR_PAR_DÉFAUT;
                }
            }
        }

        void MiseÀJour(GameTime gameTime)
        {
            float tempsÉcoulé = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TempsÉcouléDepuisMAJ += tempsÉcoulé;
            if (TempsÉcouléDepuisMAJ >= IntervalleMAJ)
            {
                OnClick();
                TempsÉcouléDepuisMAJ = 0;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            GestionSprites.Begin();
            GestionSprites.Draw(ImageBouton, RectangleDestination, Color.White);
            GestionSprites.DrawString(PoliceDeCaractères, Texte, PositionChaîne, CouleurTexte, 0, OrigineChaîne, 1f, SpriteEffects.None, 1);
            GestionSprites.End();
        }

    }
}
