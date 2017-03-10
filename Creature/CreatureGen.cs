using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace TimeLords
{
    /// <summary>
    /// Favoring Composition over Construction.
    /// </summary>
    [Serializable]
    public class CreatureStats
    {
        public static readonly CreatureStats Plant = new CreatureStats(1, new DNotation(4), 4, 0, Color.Green, "plant", "dungeon", new List<BodyPart> { });
        public static readonly CreatureStats Unknown = Plant;
            
        public byte speed, strength, dexterity, constitution, intelligence, wisdom, charisma, creatureImage, baseLevel;
        public int mass;
        public DNotation damage = new DNotation();
        public string name, habitat;
        public Color color;
        public List<BodyPart> anatomy;
        public CreatureStats(byte speed, DNotation damage, int mass, byte imageIndex, Color color,
            string name, string habitat, List<BodyPart> anatomy)
        {
            baseLevel = 0;
            this.speed = speed;
            this.damage = damage;
            this.mass = mass;

            this.creatureImage = imageIndex;
            this.color = color;

            this.name = name;
            this.habitat = habitat;
            this.anatomy = anatomy;
        }

        public CreatureStats DeepClone()
        {
            var clone = new CreatureStats(this.speed, this.damage, 
                this.mass, this.creatureImage, this.color, this.name, 
                this.habitat, this.anatomy);
            return clone;
        }
    }
    public class CreatureGen
    {
        public CreatureStats Stats;
        public short minDLevel;
        public int senseOfSmell, damageMin, damageMax, smelliness, bloodMax, ac, xpWorth;
        public int Seed;
        public Weapon weapon;
        public byte weaponChance;
        public List<Armor> armor; //A list of all armors
        public List<byte> armorChance; //A list of the chance to generate with it
        public string armorType;
        public List<Item> inventory;
        public List<byte> inventoryChance;

        // Necessary for each creature to have it's own seed, own random?
        public Random rng = new Random();
        public Dice rngDice = new Dice();

        public CreatureGen(CreatureStats stats, int seed)
        {
            Stats = stats;
            xpWorth = 1;
            Seed = seed;
            rng = new Random(seed);
            rngDice = new Dice(rng.Next());
            inventory = new List<Item>();
        }

        public CreatureGen(CreatureGen cG, int seed) : this(cG.Stats, seed)
        {	
            this.xpWorth = cG.xpWorth;
            this.rng = cG.rng;
            this.senseOfSmell = cG.senseOfSmell;
            this.smelliness = cG.smelliness;
            this.bloodMax = cG.bloodMax;
            this.weapon = cG.weapon;
            this.weaponChance = cG.weaponChance;
            this.armor = cG.armor;
            this.armorChance = cG.armorChance;
            this.minDLevel = cG.minDLevel;            
            this.inventory = cG.inventory;
        }

        public Creature GenerateCreature(string socialClass, List<Item> ItemLibrary, int rngSeed)
        {
            rng = new Random(rngSeed); //Able to generate persistent creatures            
            List<BodyPart> thisCreatureAnatomy = new List<BodyPart>();
            foreach (BodyPart b in this.Stats.anatomy)
            {
                thisCreatureAnatomy.Add(new BodyPart(b));
            }
            Creature genCreature;
            if (socialClass == "monster")
            {
                genCreature = new Creature(this);
            }
            else
            {
                genCreature = new QuestGiver(this, ItemLibrary);
            }
			genCreature.anatomy = new List<BodyPart>(); //Clear out whatever
            foreach (BodyPart b in thisCreatureAnatomy)
                genCreature.anatomy.Add(new BodyPart(b));

            if (rngDice.Roll(2) == 1) //1 in 2 chance
            {
                genCreature.gold = rngDice.Roll(10, 10); //10d10
            }
            genCreature.armorType = armorType;            
            genCreature.wornArmor = armor;
            genCreature.weapon = weapon;
            genCreature.strength = (byte)rng.Next(7, 14); //7-13
            genCreature.dexterity = (byte)rng.Next(7, 14); //7-13
            genCreature.constitution = (byte)rng.Next(7, 14); //7-13
            genCreature.hpMax = rngDice.Roll(Stats.baseLevel, 8);
            if (Stats.baseLevel <= 0)
            {
                genCreature.hpMax = rngDice.Roll(4);
            }
            genCreature.hp = genCreature.hpMax;
            genCreature.intelligence = (byte)rng.Next(7, 14); //7-13
            genCreature.wisdom = (byte)rng.Next(7, 14); //7-13
            genCreature.charisma = (byte)rng.Next(7, 14); //7-13
            genCreature.xpWorth = xpWorth;
            foreach (Item i in this.inventory)
            {
                genCreature.inventory.Add(i);
            }

            thisCreatureAnatomy.Clear();

            return genCreature;            
        }       
    }
}
