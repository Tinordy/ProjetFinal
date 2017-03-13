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
using System.Net;


namespace AtelierXNA
{

    public class MenuIPServeur : Menu
    {
        public string IP { get; private set; }
        public int Port { get; private set; }
        EntréeDeTexte LecteurPort { get; set; }
        public MenuIPServeur(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            LecteurPort = new EntréeDeTexte(Game, new Vector2(3 * Game.Window.ClientBounds.Width / 5, Game.Window.ClientBounds.Height / 3), "Arial",6);
            string HostName = Dns.GetHostName();
            IP = Dns.GetHostAddresses(HostName)[1].ToString();
            base.Initialize();
            Composantes.Add(new ArrièrePlan(Game, "Carte"));
            Composantes.Add(new Titre(Game, "Port : ", "Arial", new Vector2(Game.Window.ClientBounds.Width/5, Game.Window.ClientBounds.Height/3), "Blanc"));
            Composantes.Add(LecteurPort);
            Composantes.Add(new Titre(Game, "IP :", "Arial", new Vector2(Game.Window.ClientBounds.Width / 5, 2 * Game.Window.ClientBounds.Height / 3), "Blanc"));
            Composantes.Add(new Titre(Game, IP, "Arial", new Vector2(3 * Game.Window.ClientBounds.Width / 5, 2 * Game.Window.ClientBounds.Height / 3), "Blanc"));
            Composantes.Add(new BoutonDeCommande(Game, "OK", "Arial", "BoutonVert", "BoutonNoir", new Vector2(Game.Window.ClientBounds.Width / 2, 4 * Game.Window.ClientBounds.Height / 5), true, Lire, 0.01f));

            Activer();
        }

        private void Lire()
        {
            //gérer exception
            try
            {
                Port = int.Parse(LecteurPort.ObtenirEntrée());
            }
            catch(Exception)
            {
                //messagebox
            }
            Choix = ChoixMenu.CONNECTION;
        }
    }
}
