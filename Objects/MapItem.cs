using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tank_game
{
    //parent class for map items
    abstract class MapItem 
    {
        public int cordinateX { get; set; }
        public int cordinateY { get; set; }
    }
}
