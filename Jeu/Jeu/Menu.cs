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
    public enum ChoixMenu { EN_ATTENTE, JOUER, OPTION, QUITTER, RETOUR, PROFILE_SOLO, PROFILE_MULTI, SERVEUR }
    public class Menu : Microsoft.Xna.Framework.DrawableGameComponent
    {
        protected List<DrawableGameComponent> Composantes { get; private set; }
        public ChoixMenu Choix { get; protected set; }
        public Menu(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            Choix = ChoixMenu.EN_ATTENTE; // SI ON RE-OUVRE?????? on enabled changed
            Composantes = new List<DrawableGameComponent>();
        }
        protected void Activer()
        {
            foreach (DrawableGameComponent composante in Composantes)
            {
                Game.Components.Add(composante);
            }
            Enabled = false;
        }
        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            foreach (DrawableGameComponent composante in Composantes)
            {
                composante.Enabled = Enabled;
                composante.Visible = Enabled;
            }
            if (Enabled)
            {
                Choix = ChoixMenu.EN_ATTENTE;
            }
        }
    }
}
