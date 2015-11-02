using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tank_game
{
    //class for the player
    class Player : MapItem
    {
       
        public int direction { get; set; }
        public bool whetherShot { get; set; }
        public int health { get; set; }
        public int coins { get;set;}
        public int points { get; set; }
        public int cordinateX { get; set; }
        public int cordinateY { get; set; }
        public Player(String name)
        { this.name = name[1]+""; }

    }
}
