using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using static TimeLords.Adventurer;

namespace TimeLords
{
    public class Tile
    {
		public const float ROOMTEMP = 20; //Room temperature, in celsius: 20 celsius == 68 fahrenheit
        public ConsoleTile lastSeenTile {get;set;}
        public float temperature {get;set;}
        public string engraving {get;set;}
        public List<int> scentMagnitude {get;set;}
        public List<string> scentIdentifier {get;set;}
        public bool isInLOS, hasBeenSeen, isTransparent, isPassable, isCorridorEdge, isRoomable, isRoomEdge, isWall, isDoor;
        public bool hasBeenDug {get;set;}
        public Material material {get;set;}
        public int tileImage {get;set;}
		public int adjacentToRoomN {get;set;}
		public Point pos {get;set;}
        public List<Item> itemList {get;set;}
        public List<Fixture> fixtureLibrary {get;set;}

        public const int BlankImage = Int32.MinValue;

		public Tile():this(Material.air){}
        public Tile(Material m) : this(m.density < 1.5f, m, BlankImage, true, true)
        { }
        public Tile(bool passable, Material mat, int image, bool roomable, bool transparent)
        {
			this.itemList = new List<Item>();
			this.scentIdentifier = new List<string>();
			this.scentMagnitude = new List<int>();
			this.fixtureLibrary = new List<Fixture>();
            this.temperature = ROOMTEMP; //20 Celsius = 68 Fahrenheit

            this.isPassable = passable;
            material = mat;
            tileImage = image;
            isRoomable = roomable;
            isTransparent = transparent;
            hasBeenSeen = false;
            adjacentToRoomN = 0;
        }
		
		

        public void MakeOpen()
        {
            isTransparent = true;
            isPassable = true;
            isWall = false;
        }
    } //Tiles that the dungeon is made of
}
