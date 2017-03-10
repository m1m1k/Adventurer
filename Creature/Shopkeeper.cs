using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace TimeLords
{
    [Serializable]
    public class Shopkeeper : Creature //A creature that gives quests. What?
    {
		Room shop; //The room the shopkeeper patrols
		
        public Shopkeeper(CreatureStats stats, int rngSeed, List<Item> giveWantOptions)
            : base(stats, rngSeed)
        {                  
        }
    }
}
