using System;
using System.Collections.Generic;
using System.Text;

namespace TimeLords
{
    [Serializable]
    public class Dice
    {
        Random rng = new Random();

        public Dice()
        {         
        }
        public Dice(int seed)
        {
            rng = new Random(seed);
        }

        public int Roll(DNotation nds)
        {
            int result = 0;            

            for (int i = 0; i < nds.dNum; i++)
            {
                result += rng.Next(0, nds.sides);
                result++; //Because rng.Next(x,y) is inclusive on x but exclusive on y. YYYYYYYYYYYY
            }

            result += nds.bonus;

            return result;
        }
        public int Roll(int sides)
        {
            int result = 0;
            sides++; //Because rng.Next(x,y) is inclusive on x but exclusive on y. YYYYYYYYYY

            result += rng.Next(1, sides);

            return result;
        }
        public int Roll(int nDie, int sides)
        {
            int result = 0;
            sides++; //Because rng.Next(x,y) is inclusive on x but exclusive on y. YYYYYYYYYY            

            for (int i = 0; i < nDie; i++)
            {
                result += rng.Next(1, sides);
            }

            return result;
        }
        public int Roll(int nDie, int sides, int bonus)
        {
            int result = 0;
            sides++; //Because rng.Next(x,y) is inclusive on x but exclusive on y. YYYYYYYYYY            

            for (int i = 0; i < nDie; i++)
            {
                result += rng.Next(1, sides);
            }

            result += bonus;

            return result;
        }
    }
}
