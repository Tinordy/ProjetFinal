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
        string Message { get; set; }
        SpriteBatch GestionSprites { get; set; }
        SpriteFont ArialFont { get; set; }
        Vector2 Position { get; set; }
        string ValeurVolant { get; set; }
        float IntervalleMAJ { get; set; }
        float Temps�coul�DepuisMAJ { get; set; }
        Device ElVolant { get; set; }
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
/// 32 767 veut dire aucune acc�l�ration (aucun mouvement de la p�dale)
/// 0 veut dire que la p�dale (Dave) est dans le fond
/// </summary>
public int AxeY 
        {
            get { return ElVolant.CurrentJoystickState.Y; }
        }
        /// <summary>
        /// 65 535 veut dire aucun freinage (aucun mouvement de la p�dale)
        /// 0 veut dire que la p�dale (Dave) est dans le fond
        /// </summary>
        public int AxeZ
        {
            get { return ElVolant.CurrentJoystickState.Z; }
        }

        public Volant(Game game, string message, float intervalleMAJ)
            : base(game)
        {
            Message = message;
            IntervalleMAJ = intervalleMAJ;
        }
        public override void Initialize()
        {
            ChargerVolant();
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            float temps�coul� = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Temps�coul�DepuisMAJ += temps�coul�;


            if (Temps�coul�DepuisMAJ >= IntervalleMAJ)
            {
                Temps�coul�DepuisMAJ = 0;
            }
            base.Update(gameTime);
        }


        public void ChargerVolant()
        {
            //Find all the GameControl devices that are attached.
            try
            {
                DeviceList gameControllerList = Manager.GetDevices(DeviceType.Joystick, EnumDevicesFlags.AttachedOnly);
                // check that we have at least one device.
                if (gameControllerList.Count > 0)
                {
                    // Move to the first device
                    gameControllerList.MoveNext();
                    DeviceInstance deviceInstance = (DeviceInstance)gameControllerList.Current;

                    // create a device from this controller.
                    ElVolant = new Device(deviceInstance.InstanceGuid);
                    //Volant.SetCooperativeLevel(CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                }

                // Tell DirectX that this is a Joystick.
                ElVolant.SetDataFormat(DeviceDataFormat.Joystick);
                // Finally, acquire the device.
                ElVolant.Acquire();
            }
            catch (System.Exception e) { }
        }
    }
}
