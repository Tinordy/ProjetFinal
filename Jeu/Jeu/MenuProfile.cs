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
using System.Net;
using System.IO;

namespace AtelierXNA
{
    public class MenuProfile : Menu
    {
        public int Voiture { get; private set; }
        public string Pseudonyme { get; set; }
        Entr�eDeTexte LecteurPseudonyme { get; set; }
        D�fileurSprite ChoixVoiture { get; set; }
        BoutonDeCommande BtnValider { get; set; }
        BoutonDeCommande BtnD�marrer { get; set; }
        Server Serveur { get; set; }
        R�seautique NetworkManager { get; set; }
        BinaryWriter writer { get; set; }
        MemoryStream writeStream { get; set; }
        public MenuProfile(Game game)
              : base(game)
        { }

        public override void Initialize()
        {
            List<string> noms = new List<string>();
            noms.Add("choix1");
            noms.Add("Neige");
            noms.Add("choix3");
            LecteurPseudonyme = new Entr�eDeTexte(Game, new Vector2(3 * Game.Window.ClientBounds.Width / 4, Game.Window.ClientBounds.Height / 4), "Arial20", 10);
            ChoixVoiture = new D�fileurSprite(Game, noms, new Rectangle(0, Game.Window.ClientBounds.Height / 3, Game.Window.ClientBounds.Width, 2 * Game.Window.ClientBounds.Height / 3), 0.001f);
            BtnD�marrer = new BoutonDeCommande(Game, "D�marrer", "Arial", "BoutonVert", "BoutonNoir", new Vector2(2 * Game.Window.ClientBounds.Width / 3, 8 * Game.Window.ClientBounds.Height / 9), false, D�marrer, 0.01f);
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
            //pas plus tot car cr�� au d�but... changer ordre??
            NetworkManager = Game.Services.GetService(typeof(R�seautique)) as R�seautique;
            Serveur = Game.Services.GetService(typeof(Server)) as Server;

            Choix = ChoixMenu.VALIDATION;
            Pseudonyme = LecteurPseudonyme.ObtenirEntr�e();
            Voiture = ChoixVoiture.DonnerChoixVoiture();
            NetworkManager.SetChoixVoiture(Voiture);
            BtnValider.EstActif = false;
            foreach(DrawableGameComponent s in Composantes)
            {
                if(s is IS�lectionnable)
                {
                    s.Enabled = false;
                }
            }
            //d�sactiver le reste (pseudonyme et voiture)
        }
        private void D�marrer()
        {
            //INITIALISATION?

            //SERVEUR -- Pr�t � jouer
            NetworkManager.SendPr�tJeu(true);
            //NetworkManager.writeStream.Position = 0;
            //NetworkManager.writer.Write((byte)Protocoles.ReadyToPlayChanged);
            //NetworkManager.writer.Write(true);
            //NetworkManager.SendData(Serveur.GetDataFromMemoryStream(NetworkManager.writeStream));
            Choix = ChoixMenu.JOUER;
        }
        public void ActiverBtnD�marrer()
        {
            BtnD�marrer.EstActif = true;
            Choix = ChoixMenu.EN_ATTENTE;
        }
        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            base.OnEnabledChanged(sender, args);
            BtnD�marrer.EstActif = false;
        }
    }
}
