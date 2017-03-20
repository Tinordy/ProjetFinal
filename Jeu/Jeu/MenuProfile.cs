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
        public string Pseudonyme { get; private set; }
        EntréeDeTexte LecteurPseudonyme { get; set; }
        DéfileurSprite ChoixVoiture { get; set; }
        BoutonDeCommande BtnValider { get; set; }
        BoutonDeCommande BtnDémarrer { get; set; }
        Server Serveur { get; set; }
        Réseautique NetworkManager { get; set; }
        BinaryWriter writer { get; set; }
        MemoryStream writeStream { get; set; }
        public MenuProfile(Game game)
              : base(game)
        { }

        public override void Initialize()
        {
            NetworkManager = Game.Services.GetService(typeof(Réseautique)) as Réseautique;
            Serveur = Game.Services.GetService(typeof(Server)) as Server;
            List<string> noms = new List<string>();
            noms.Add("CielÉtoilé");
            noms.Add("Neige");
            noms.Add("brique1");
            LecteurPseudonyme = new EntréeDeTexte(Game, new Vector2(3 * Game.Window.ClientBounds.Width / 4, Game.Window.ClientBounds.Height / 4), "Arial20", 10);
            ChoixVoiture = new DéfileurSprite(Game, noms, new Rectangle(0, Game.Window.ClientBounds.Height / 3, Game.Window.ClientBounds.Width, 2 * Game.Window.ClientBounds.Height / 3), 0.01f);
            BtnDémarrer = new BoutonDeCommande(Game, "Démarrer", "Arial", "BoutonVert", "BoutonNoir", new Vector2(2 * Game.Window.ClientBounds.Width / 3, 8 * Game.Window.ClientBounds.Height / 9), false, Démarrer, 0.01f);
            BtnValider = new BoutonDeCommande(Game, "Valider", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 3, 8 * Game.Window.ClientBounds.Height / 9), true, Valider, 0.01f);
            base.Initialize();
            Composantes.Add(new ArrièrePlan(Game, "MenuOption"));
            Composantes.Add(new Titre(Game, "Pseudonyme: ", "Arial", new Vector2(Game.Window.ClientBounds.Width / 4, Game.Window.ClientBounds.Height / 4), "Blanc"));
            Composantes.Add(LecteurPseudonyme);
            Composantes.Add(ChoixVoiture);
            Composantes.Add(BtnDémarrer);
            Composantes.Add(BtnValider);

            Activer();
        }

        private void Valider()
        {
            Choix = ChoixMenu.VALIDATION;
            Pseudonyme = LecteurPseudonyme.ObtenirEntrée();
            Voiture = ChoixVoiture.DonnerChoixVoiture();
            BtnValider.EstActif = false;
            //désactiver le reste (pseudonyme et voiture)
        }
        private void Démarrer()
        {
            //INITIALISATION?

            //SERVEUR -- Prêt à jouer
            NetworkManager.writeStream.Position = 0;
            NetworkManager.writer.Write((byte)Protocoles.ReadyToPlayChanged);
            NetworkManager.writer.Write(true);
            NetworkManager.SendData(Serveur.GetDataFromMemoryStream(NetworkManager.writeStream));
            Choix = ChoixMenu.JOUER;
        }
        public void ActiverBtnDémarrer()
        {
            BtnDémarrer.EstActif = true;
        }
    }
}
