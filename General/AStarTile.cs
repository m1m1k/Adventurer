using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TimeLords
{
    public struct AStarTile
    {
        public Point pos, parentpos;
        public int f, g, h, w;

        public AStarTile(Point xy, Point parent, int gz, int hz, int wz)
        {
            pos = xy; //This tile's position
            parentpos = parent; //Parent tile's position
            w = wz;
            g = gz;
            h = hz;
            f = g + h;
        }
        public AStarTile(Point xy, Point parent, int gz, int hz)
        {
            pos = xy; //This tile's position
            parentpos = parent; //Parent tile's position
            w = 100;
            g = gz;
            h = hz;
            f = g + h;
        }
    }
}
