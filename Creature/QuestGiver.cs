using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using TimeLords;

namespace TimeLords
{
    [Serializable]
    class QuestGiver : Creature //A creature that gives quests. What?
    {
        public string wantObject, giveObject;

        public QuestGiver(CreatureGen gen, List<Item> giveWantOptions) 
            : this(gen.Stats, gen.rng.Next(), giveWantOptions)
        { }
        public QuestGiver(CreatureStats stats, int rngSeed, List<Item> giveWantOptions)
            :base(stats, rngSeed)
        {
            rng = new Random(rngSeed); //Persistence possible
            var randomOption = giveWantOptions.ChooseRandom();
            if (randomOption != null)
            {
                wantObject = randomOption.name;
            }
            giveObject = wantObject;

            while (wantObject == giveObject && giveWantOptions.Count > 1)
            {
                Item giveItem = giveWantOptions.ChooseRandom();
                giveObject = giveItem.name;
                inventory.Add(giveItem); //Make sure s/he actually has the item.
            }                    
        }

        //public QuestGiver(QuestGiver c)
        //    : base(c)
        //{
        //    this.wantObject = c.wantObject;
        //    this.giveObject = c.giveObject;
        //}

        public void CycleWantGiveItem(List<Item> giveWantOptions)
        {
            wantObject = giveWantOptions[rng.Next(0, giveWantOptions.Count)].name;

            giveObject = wantObject;

            while (wantObject == giveObject && giveWantOptions.Count > 1)
            {
                Item giveItem = inventory[rng.Next(0, inventory.Count)];
                giveObject = giveItem.name;
            }
        }
    }
}
