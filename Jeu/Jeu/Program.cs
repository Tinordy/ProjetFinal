using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace AtelierXNA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (Atelier game = new Atelier())
            {
                game.Run();
            }
        }
    }
}
