using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace mustank.bean
{
    class BrickWall
    {
        Point pos;

        public Point Pos
        {
            get { return pos; }
            set { pos = value; }
        }
        int damageLevel = 0;

        public int DamageLevel
        {
            get { return damageLevel; }
            set { damageLevel = value; }
        }

        public BrickWall(Point position)
        {
            pos = position;
        }
    }
}
