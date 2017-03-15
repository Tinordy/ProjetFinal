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
using System.Windows.Forms;





namespace AtelierXNA
{
    public class MenuProfile : Menu
    {
        public int Voiture { get; private set; }
        public string Pseudonyme { get; private set; }
        Entr�eDeTexte LecteurPseudonyme { get; set; }
        D�fileurSprite ChoixVoiture { get; set; }
        BoutonDeCommande BtnValider { get; set; }
        BoutonDeCommande BtnD�marrer { get; set; }
        Server Serveur { get; set; }
        public MenuProfile(Game game)
            : base(game)
        { }

        public override void Initialize()
        {
            Serveur = Game.Services.GetService(typeof(Server)) as Server;
            List<string> noms = new List<string>();
            noms.Add("Ciel�toil�");
            noms.Add("Neige");
            noms.Add("brique1");
            LecteurPseudonyme = new Entr�eDeTexte(Game, new Vector2(3 * Game.Window.ClientBounds.Width / 4, Game.Window.ClientBounds.Height / 4), "Arial20", 10);
            ChoixVoiture = new D�fileurSprite(Game, noms, new Rectangle(0, Game.Window.ClientBounds.Height / 3, Game.Window.ClientBounds.Width, 2 * Game.Window.ClientBounds.Height / 3), 0.01f);
            BtnD�marrer = new BoutonDeCommande(Game, "D�marrer", "Arial", "BoutonVert", "BoutonNoir", new Vector2(2*Game.Window.ClientBounds.Width / 3, 8 * Game.Window.ClientBounds.Height / 9), false, D�marrer, 0.01f);
            BtnValider = new BoutonDeCommande(Game, "Valider", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 3, 8 * Game.Window.ClientBounds.Height / 9), true, Valider, 0.01f);
            base.Initialize();
            Composantes.Add(new Arri�rePlan(Game, "MenuOption"));
            Composantes.Add(new Titre(Game, "Pseudonyme: ", "Arial", new Vector2(Game.Window.ClientBounds.Width / 4, Game.Window.ClientBounds.Height / 4), "Blanc"));
            Composantes.Add(LecteurPseudonyme);
            Composantes.Add(ChoixVoiture);
            Composantes.Add(BtnD�marrer);
            Composantes.Add(BtnValider);

            Activer();
        }

        private void Valider()
        {
            Choix = ChoixMenu.VALIDATION;
            Pseudonyme = LecteurPseudonyme.ObtenirEntr�e();
            Voiture = ChoixVoiture.DonnerChoixVoiture();
            BtnValider.EstActif = false;
        }
        private void D�marrer()
        {
            //INITIALISATION?

            //SERVEUR - - Pr�t � jouer!

            Choix = ChoixMenu.JOUER;
        }
        public void ActiverBtnD�marrer()
        {
            BtnD�marrer.EstActif = true;
        }
    }
}
