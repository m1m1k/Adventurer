using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TimeLords
{
    public class Corridor
    {
        public Point pointA {get;protected set;}
        public Point pointB {get;protected set;}

		public Corridor():this(new Point(), new Point()){}
        public Corridor(Point a, Point b)
        {
            this.pointA = a;
            this.pointB = b;
        }
		public Corridor(Corridor c)
		{
			this.pointA = c.pointA;
			this.pointB = c.pointB;
		}
    } //A hallway through the dungeon
}
