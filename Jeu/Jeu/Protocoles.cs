﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AtelierXNA
{
    public enum Protocoles
    {
        Disconnected = 0,
        Connected = 1,
        PlayerMoved = 2,
        PositionInitiale = 3,
        ReadyToPlayChanged = 4,
        HasArrivedToEnd = 5,
        ChoseCar = 6
    }
}
