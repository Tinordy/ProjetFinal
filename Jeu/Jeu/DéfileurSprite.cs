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
    public class DéfileurSprite : DrawableGameComponent, ISélectionnable
    {
        List<Texture2D> Textures { get; set; }
        List<string> NomTextures { get; set; }
        RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }
        SpriteBatch GestionSprite { get; set; }
        Rectangle DestinationSprite { get; set; }
        Rectangle RégionFlècheD { get; set; }
        Texture2D TextureFlècheD { get; set; }
        Texture2D TextureFlècheG { get; set; }
        Rectangle RégionFlècheG { get; set; }
        Rectangle Région { get; set; }
        Rectangle DestinationI { get; set; }
        Rectangle DestinationF { get; set; }
        Rectangle? SourceI { get; set; }
        Rectangle? SourceF { get; set; }
        InputManager GestionInput { get; set; }
        int IndexI { get; set; }
        int IndexF { get; set; }
        int NbTextures { get; set; }
        float IntervalleMAJ { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }
        bool DéroulerD { get; set; }
        bool DéroulerG { get; set; }

        public bool EstSélectionné { get; set; } 
        public bool EstActif { get; set; }

        public DéfileurSprite(Game game, List<string> nomTextures, Rectangle destination, float intervalleMAJ)
            : base(game)
        {
            NomTextures = nomTextures;
            Région = destination;
            IntervalleMAJ = intervalleMAJ;
        }

        public override void Initialize()
        {
            EstActif = true;
            SourceI = null;
            SourceF = null;
            DéroulerG = false;
            DéroulerD = false;
            IndexI = 0;
            IndexF = 0;
            NbTextures = NomTextures.Count;
            TempsÉcouléDepuisMAJ = 0;
            Textures = new List<Texture2D>();
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            GestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            foreach (string nom in NomTextures)
            {
                Textures.Add(GestionnaireDeTextures.Find(nom));
            }
            TextureFlècheD = GestionnaireDeTextures.Find("Right");
            TextureFlècheG = GestionnaireDeTextures.Find("Left");
            GestionSprite = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
            DestinationSprite = new Rectangle(Région.X + Région.Width / 8, Région.Y + Région.Height / 8, 3 * Région.Width / 4, 3 * Région.Height / 4);
            RégionFlècheD = new Rectangle(Région.X + 8 * Région.Width / 9, Région.Y + Région.Height / 2, Région.Width / 9, Région.Height / 9);
            RégionFlècheG = new Rectangle(Région.X, Région.Y + Région.Height / 2, Région.Width / 9, Région.Height / 9);
            DestinationI = DestinationSprite;
            DestinationF = new Rectangle(DestinationSprite.X + DestinationSprite.Width, DestinationSprite.Y, 0, DestinationSprite.Height);
        }
        public override void Update(GameTime gameTime)
        {
            if(EstActif)
            {
                GérerSouris();
                float tempsÉcoulé = (float)gameTime.ElapsedGameTime.TotalSeconds;
                TempsÉcouléDepuisMAJ += tempsÉcoulé;
                if (TempsÉcouléDepuisMAJ > IntervalleMAJ)
                {
                    if (DéroulerD)
                    {
                        DestinationI = new Rectangle(DestinationI.X, DestinationI.Y, DestinationI.Width - 2, DestinationI.Height);
                        DestinationF = new Rectangle(DestinationF.X - 2, DestinationF.Y, DestinationF.Width + 2, DestinationF.Height);
                        if (DestinationF.Width == DestinationSprite.Width)
                        {
                            DéroulerD = false;
                            DestinationI = DestinationF;
                            IndexI = IndexF;
                        }
                    }
                    if (DéroulerG)
                    {
                        DestinationI = new Rectangle(DestinationI.X + 2, DestinationI.Y, DestinationI.Width - 2, DestinationI.Height);
                        DestinationF = new Rectangle(DestinationF.X, DestinationF.Y, DestinationF.Width + 2, DestinationF.Height);
                        if (DestinationF.Width == DestinationSprite.Width)
                        {
                            DéroulerG = false;
                            DestinationI = DestinationF;
                            IndexI = IndexF;
                        }
                    }
                    TempsÉcouléDepuisMAJ = 0;
                }
            }
        }

        private void GérerSouris()
        {
            Point positionSouris = GestionInput.GetPositionSouris();
            if (RégionFlècheD.Contains(positionSouris))
            {
                if (GestionInput.EstNouveauClicGauche())
                {
                    IndexF = (IndexI + 1) % NbTextures;
                    DéroulerD = true;
                    DéroulerG = false;
                    DestinationF = new Rectangle(DestinationSprite.X + DestinationSprite.Width, DestinationSprite.Y, 0, DestinationSprite.Height);
                }
            }
            if (RégionFlècheG.Contains(positionSouris))
            {
                if (GestionInput.EstNouveauClicGauche())
                {
                    IndexF = IndexI == 0 ? NbTextures - 1 : IndexI - 1;
                    DéroulerG = true;
                    DéroulerD = false;
                    DestinationF = new Rectangle(DestinationSprite.X, DestinationSprite.Y, 0, DestinationSprite.Height);

                }
            }

        }

        public override void Draw(GameTime gameTime)
        {
            GestionSprite.Begin();
            GestionSprite.Draw(Textures[IndexI], DestinationI, SourceI, Color.White);
            GestionSprite.Draw(Textures[IndexF], DestinationF, SourceF, Color.White);

            GestionSprite.Draw(TextureFlècheD, RégionFlècheD, Color.White);
            GestionSprite.Draw(TextureFlècheG, RégionFlècheG, Color.White);
            GestionSprite.End();
        }
        public int DonnerChoix()
        {
            return IndexF;
        }
    }
}
