using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Tao.Sdl;

namespace TimeLords
{
    [Serializable]
    public class Item
    {
        public static readonly Item Unknown = new Item("stick", Color.Brown);

        public DNotation damage {get;set;}
        public float mass {get;set;}
        public bool edible {get;set;}
		public bool bladed {get;set;}
        public float volume {get;set;}
        public int nutrition {get;set;}
        public int itemImage {get;set;}
        public string name {get;set;}
        public List<string> use {get;set;} //A list of things that can be done with this item
        public Color color {get;set;}
        public List<Item> componentList {get;set;}
        public Effect eatEffect {get;set;}
		
		protected Material materialRaw;
		public Material material 
		{
			get
			{
				if (componentList != null)
				{
					if (componentList.Count > 0)
						return componentList[0].material;
					else 
						return materialRaw;
				}
				else
					return materialRaw;
			}
			
			set
			{	
				if (componentList != null)
				{
					if (componentList.Count > 0)
						componentList[0].material = value;
					else
						materialRaw = value;
					
				}
				else
					materialRaw = value;
			}
		}

		public Item() :this("missingno(Item)"){}
		public Item(string name) :this(1f, 1f, name){}
		public Item(string name, Color color) :this(1f, 1f, name, color){}
		public Item(string name, Color color, List<Item> componentList) :this(1f, 1f, name, color, componentList){}
        public Item(float mass, float volume, string name) :this(mass, volume, name, Color.White){}
		public Item(float mass, float volume, string name, Color color):this(mass,volume,name,color, new List<Item>(), new List<string>()){}
		public Item(float mass, float volume, string name, Color color, List<Item> componentList)
			:this(mass,volume,name,color, componentList, new List<string>()){}
		public Item(float mass, float volume, string name, Color color, List<Item> componentList, List<string> use)
		{			
			this.componentList = new List<Item>();
			foreach (Item i in componentList)
				this.componentList.Add(CopyDeep(i));
			
			this.use = use;
			this.edible = false;
			this.mass = mass;
			this.volume = volume;
			this.name = name;
			this.color = color;
			this.itemImage = 40;
			this.damage = new DNotation(2);
			this.material = new Material();
		}
        public Item(Item i)
        {			
			this.material = new Material(i.material);
            this.edible = i.edible;
            this.damage = new DNotation(i.damage);
            this.mass = i.mass;
            this.volume = i.volume;
            this.itemImage = i.itemImage;
            this.name = i.name;
            this.color = i.color;
			this.use = new List<string>();
			foreach(string s in i.use)
            	this.use.Add(s);
			this.componentList = new List<Item>();
			foreach(Item t in i.componentList)
            	this.componentList.Add(Item.CopyDeep(t)); //Recursion until we hit the base components
            this.eatEffect = i.eatEffect;
        }

		public static Item CopyDeep(Item i)
		{
			if (i is Amulet)
				return new Amulet((Amulet)i);
			else if (i is Armor)
				return new Armor((Armor)i);
			else if (i is Currency)
				return new Currency((Currency)i);
			else if (i is Potion)
				return new Potion((Potion)i);
			else if (i is Ring)
				return new Ring((Ring)i);
			else if (i is Scroll)
				return new Scroll((Scroll)i);
			else if (i is Weapon)
				return new Weapon((Weapon)i);
			else
				return new Item(i);
		}
        public virtual Level Eat(Level currentLevel, Creature consumer)
        {
            int creatureIndex = currentLevel.creatureList.IndexOf(consumer);

            if (!edible)
            {
                consumer.hp -= damage.dNum * damage.sides + damage.bonus; //Full damage
                currentLevel.creatureList[creatureIndex].message.Add("That was less than edible.");
                currentLevel.causeOfDeath = "internal damage to your stomach";
                currentLevel.mannerOfDeath = "you swallowed a " + name;
            }

            consumer.food += nutrition;
            currentLevel.creatureList[creatureIndex].inventory.Remove(this);
            return currentLevel;
        }

        //String representation of an atom should be its name
        public override string ToString()
        {
            return name;
        }
    } //It's a thing that things can pick up. Thing.
}
