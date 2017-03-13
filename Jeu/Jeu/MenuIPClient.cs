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
    public class MenuIPClient : Menu
    {
        public int Port { get; private set; }
        public string IP { get; private set; }
        Entr�eDeTexte LecteurPort { get; set; }
        Entr�eDeTexte LecteurIP { get; set; }
        public MenuIPClient(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            LecteurPort = new Entr�eDeTexte(Game, new Vector2(3 * Game.Window.ClientBounds.Width / 5, Game.Window.ClientBounds.Height / 3), "Arial",6);
            LecteurIP = new Entr�eDeTexte(Game, new Vector2(3 * Game.Window.ClientBounds.Width / 5, 2 * Game.Window.ClientBounds.Height / 3), "Arial",14);
            base.Initialize();
            Composantes.Add(new Arri�rePlan(Game, "Carte"));
            Composantes.Add(new Titre(Game, "Port : ", "Arial", new Vector2(Game.Window.ClientBounds.Width / 5, Game.Window.ClientBounds.Height / 3), "Blanc"));
            Composantes.Add(LecteurPort);
            Composantes.Add(new Titre(Game, "IP :", "Arial", new Vector2(Game.Window.ClientBounds.Width / 5, 2 * Game.Window.ClientBounds.Height / 3), "Blanc"));
            Composantes.Add(LecteurIP);
            Composantes.Add(new BoutonDeCommande(Game, "OK", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 2, 4 * Game.Window.ClientBounds.Height / 5), true, Lire, 0.01f));

            Activer();
        }
        void Lire()
        {
            Port = int.Parse(LecteurPort.ObtenirEntr�e());
            IP = LecteurIP.ObtenirEntr�e();
            Choix = ChoixMenu.CONNECTION;
        }
    }
}
