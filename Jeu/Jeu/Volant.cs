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
using Microsoft.DirectX.DirectInput;



namespace AtelierXNA
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Volant : Microsoft.Xna.Framework.GameComponent
    {
        const int BOUTON_PRESSED = 128;
        Vector2 Position { get; set; }
        string ValeurVolant { get; set; }
        float IntervalleMAJ { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }
        Device ElVolant { get; set; }
        Device ElButtons { get; set; }
        /// <summary>
        /// 0 volant vers la gauche
        /// 65 535 volant vers la droite
        /// 32 767 volant au centre
        /// </summary>
        public int AxeX
        {
            get { return ElVolant.CurrentJoystickState.X; }
        }
        /// <summary>
        /// 32 767 veut dire aucune accélération (aucun mouvement de la pédale)
        /// 0 veut dire que la pédale (Dave) est dans le fond
        /// </summary>
        public int AxeY
        {
            get { return ElVolant.CurrentJoystickState.Y == 65535 ? 32767 : ElVolant.CurrentJoystickState.Y; }
        }
        /// <summary>
        /// 65 535 veut dire aucun freinage (aucun mouvement de la pédale)
        /// 0 veut dire que la pédale (Dave) est dans le fond
        /// </summary>
        public int AxeZ
        {
            get { return ElVolant.CurrentJoystickState.Rz; }
        }
        public bool BoutonDérapageActivé
        {
            get
            {
                return Buttons[6] == BOUTON_PRESSED || Buttons[7] == BOUTON_PRESSED;
            }
        }

        public List<int> Buttons { get; set; }
        

        public Volant(Game game, float intervalleMAJ)
            : base(game)
        {
            IntervalleMAJ = intervalleMAJ;
        }
        public override void Initialize()
        {
            Buttons = new List<int>(8);
            ChargerVolant();
            //ChargerButtons();
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            float tempsÉcoulé = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TempsÉcouléDepuisMAJ += tempsÉcoulé;


            if (TempsÉcouléDepuisMAJ >= IntervalleMAJ)
            {
                UpdateButtons();
                TempsÉcouléDepuisMAJ = 0;
                //Game.Window.Title = AxeX.ToString() + " / " + AxeY.ToString() + " / " + AxeZ.ToString();

            }
            base.Update(gameTime);
        }

        void UpdateButtons()
        {
            int NumButton = 0;
            Buttons.Clear();
            byte[] tableauByte = ElVolant.CurrentJoystickState.GetButtons();
            
            foreach (byte bouton in tableauByte)
            {
                if (NumButton < 8)
                {
                    Buttons.Add(bouton);
                    ++NumButton;
                }
            }
        }
        public void ChargerVolant()
        {
            //Find all the GameControl devices that are attached.

            DeviceList gameControllerList = Manager.GetDevices(DeviceType.Joystick, EnumDevicesFlags.AttachedOnly);
            // check that we have at least one device.
            if (gameControllerList.Count > 0)
            {
                try
                {
                    // Move to the first device
                    gameControllerList.MoveNext();
                    DeviceInstance deviceInstance = (DeviceInstance)gameControllerList.Current;

                    // create a device from this controller.
                    ElVolant = new Device(deviceInstance.InstanceGuid);
                    //Volant.SetCooperativeLevel(CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);

                    // Tell DirectX that this is a Joystick.
                    ElVolant.SetDataFormat(DeviceDataFormat.Joystick);
                    // Finally, acquire the device.
                    ElVolant.Acquire();
                    //ElButtons.Acquire();
                }
                catch (Exception e) { }
            }

            else { this.Enabled = !this.Enabled; }


        }

    }
}

