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
    public class MenuOption : Menu
    {
        Sprite Carte { get; set; }
        InputManager GestionInput { get; set; }
        bool Attente { get; set; }
        DéfileurSprite ChoixDifficulté { get; set; }
        public MenuOption(Game game)
            : base(game)
        { }

        public override void Initialize()
        {
            Rectangle destination = new Rectangle(Game.Window.ClientBounds.Width / 8, Game.Window.ClientBounds.Height / 8, 3 * Game.Window.ClientBounds.Width / 4, 3 * Game.Window.ClientBounds.Height / 4);
            List<string> noms = new List<string>();
            noms.Add("facile");
            noms.Add("moyen");
            noms.Add("difficile");
            ChoixDifficulté = new DéfileurSprite(Game, noms, new Rectangle(Game.Window.ClientBounds.Width/8, Game.Window.ClientBounds.Height/2,3*Game.Window.ClientBounds.Width/4, Game.Window.ClientBounds.Height/5), 0.001f);
            Attente = false;
            Carte = new Sprite(Game, "mappe", destination);
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            base.Initialize();
            Composantes.Add(new Sprite(Game, "menuOption", destination));
            Composantes.Add(new Titre(Game, "Options", "Arial", new Vector2(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 4), "Blanc", false, Color.White));
            Composantes.Add(new BoutonDeCommande(Game, "Retour", "Arial", "BoutonVert", "BoutonNoir", new Vector2(3 * Game.Window.ClientBounds.Width / 4, 3 * Game.Window.ClientBounds.Height / 4), true, Retour, 0.01f));
            Composantes.Add(new BoutonDeCommande(Game, "Afficher parcours", "Arial", "BoutonVert", "BoutonNoir", new Vector2(2*Game.Window.ClientBounds.Width / 5, 2 * Game.Window.ClientBounds.Height / 5), true, AfficherCarte, 0.01f));
            Composantes.Add(ChoixDifficulté);
            Composantes.Add(Carte);
            Activer();
            Carte.Visible = false;
        }
        public override void Update(GameTime gameTime)
        {
            if(Carte.Visible && GestionInput.EstNouveauClicGauche())
            {
                Carte.Visible = false;
                Attente = true;
            }
            else
            {
                if(Attente)
                {
                    ChangerAvtivationBoutons(true);
                    ChoixDifficulté.Enabled = true;
                    Attente = false;
                }
            }

        }
        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            base.OnEnabledChanged(sender, args);
            Carte.Visible = false;
        }
        
        private void AfficherCarte()
        {
            Carte.Visible = true;
            ChangerAvtivationBoutons(false);
            ChoixDifficulté.Enabled = false;
        }

        public void Retour()
        {
            Choix = ChoixMenu.RETOUR;
        }
        public void DésactiverDifficulté()
        {
            ChoixDifficulté.EstActif = false;
        }
        public int GetDifficulté()
        {
            return ChoixDifficulté.DonnerChoix();
        }
    }
}
