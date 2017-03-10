using System;
using System.Collections.Generic;
using System.Text;

namespace TimeLords
{
    [Serializable]
    public class DNotation
    {
        public int dNum {get;set;}
        public int sides {get;set;}
        public int bonus {get;set;}
		
		public int lower
		{
			get
			{
				return dNum + bonus;
			}
		}
		public int average 
		{
			get
			{
				return (lower + upper) / 2;
			}
		}
		public int upper
		{
			get
			{
				return dNum * sides + bonus;
			}
		}
		
		public DNotation():this(1){}
        public DNotation(int sides):this(1,sides){}
        public DNotation(int dNum, int sides):this(dNum, sides, 0){}
        public DNotation(int dNum, int sides, int bonus)
        {
            this.dNum = dNum;
            this.sides = sides;
            this.bonus = bonus;
        }
		public DNotation(DNotation d)
        {
            this.dNum = d.dNum;
            this.sides = d.sides;
            this.bonus = d.bonus;
        }
    }
}
