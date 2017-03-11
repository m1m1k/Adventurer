using System; //General C# functions
using System.Collections.Generic; //So I can use List
using System.Drawing; //The colors, man
using System.Linq;
using System.Runtime.InteropServices; //For use in wrangling pointers in their place
using Tao.Sdl;
using TimeLords;
using TimeLords.Creature;
using TimeLords.General;

namespace TimeLords
{
	public partial class Adventurer
	{
		#region Variables
		public const byte TILEWIDTH = 16; //The width of tiles in pixels
        public const byte TILEHEIGHT = 16; //The height of tiles in pixels
		
		public static Sdl.SDL_Surface screenData; //The screen bitmap        
        public static Sdl.SDL_Surface[] imageData = new Sdl.SDL_Surface[256]; //The image bitmaps
        public static IntPtr screen; //Pointer to screen bitmap
        public static IntPtr[] image = new IntPtr[256]; //Pointer to image data
        public static Sdl.SDL_Rect screenArea; //The area of the screen
        public static Sdl.SDL_Rect source; //A source rectangle to pull an image from
        public static Sdl.SDL_Rect target; //A target rectangle for where to draw it to
        public static int windowSizeX = 900; //The horizontal size of the screen
        public static int windowSizeY = 750; //The vertical size of the screen
		
        public static SdlTtf.TTF_Font veraData; //The data of the font
        public static FontFamily vera = FontFamily.GenericMonospace; //The pointer to the font
        public static SdlTtf.TTF_Font veraSmallData; //The data of the font
        public static FontFamily veraSmall = FontFamily.GenericMonospace; //The pointer to the font
		#endregion

        static void ClearAndDraw()
        {
			{
			case GameState.OpeningMenu:
				Draw_Opening();
				break;
				
			case GameState.NameSelect:
				Draw_Name();
				break;
				
			case GameState.CreatureSelect:
				Draw_CreatureSel();
				break;
				
			case GameState.HelpMenu:
				Draw_Help();
				break;
				
			case GameState.MainGame:
				Draw_Main();
				break;
				
			case GameState.InventoryMenu:
				Draw_Inventory();
				break;
				
			case GameState.HealthMenu:
				Draw_Health();
				break;
				
			case GameState.WaitForPosition:
				Draw_GetPos();
				break;
				
			case GameState.EscapeMenu:
				Draw_Escape();
				break;
			}
			
            ////Sdl.SDL_Flip(screen); //Update screen
        } //Draws things to the screen
		
        public enum OpeningMenuOptions
        {
            Start_Game = 1,
            Help = 2,
            Quit = 3
        }
		static void Draw_Opening()
        {
            ClearScreen();
            DrawText(vera, "Adventurer 0.0.2.8", new Point(windowSizeX / 2 - 130, 20), Color.White);
            DrawText(vera, "by", new Point(windowSizeX / 2 - 50, 50), Color.White);
            DrawText(vera, "Kalasen Zyphurus", new Point(windowSizeX / 2 - 130, 80), Color.White);
            DrawMenuOptions();
        }

        private static void DrawMenuOptions()
        {
            string selectedText = Enum.GetName(typeof(OpeningMenuOptions), (OpeningMenuOptions)selectionCursor);
            foreach (var option in Enum.GetNames(typeof(OpeningMenuOptions)))
            {
                if (option == selectedText)
                {
                    DrawText(vera, string.Format("<{0}>", option), new Point(350, windowSizeY / 2), Color.White);
                }
                else
                {
                    DrawText(vera, string.Format("{0}", option), new Point(350, windowSizeY / 2), Color.Gray);
                }
            }
        }

        static void Draw_Name()
		{
	        DrawText(vera, "Name Select", new Point(windowSizeX / 2 - 130, 20), Color.White);
            DrawText(veraSmall, "Enter name: " + sessionName, new Point(windowSizeX / 2 - 130, windowSizeY / 2), Color.White);
		}
		static void Draw_CreatureSel()
		{
            DrawText(vera, "Creature Select", new Point(350, 20)); //Draw the title

            int m = 60;
            Queue<string> items = new Queue<string>();
            foreach (CreatureGen item in bestiary)
            {
                if (items.Count >= 26)
                {
                    items.Dequeue(); //Don't let there be more than 26 in the queue
                }

                items.Enqueue(CapitalizeFirst(item.Stats.name));
            }

            int count = items.Count;
            for (int c = 1; c <= count; c++)
            {
                m += 15; //Skip down 15 pixels

                #region Get item letter
                string s = "a";
                if (c == 2)
                    s = "b";
                if (c == 3)
                    s = "c";
                if (c == 4)
                    s = "d";
                if (c == 5)
                    s = "e";
                if (c == 6)
                    s = "f";
                if (c == 7)
                    s = "g";
                if (c == 8)
                    s = "h";
                if (c == 9)
                    s = "i";
                if (c == 10)
                    s = "j";
                if (c == 11)
                    s = "k";
                if (c == 12)
                    s = "l";
                if (c == 13)
                    s = "m";
                if (c == 14)
                    s = "n";
                if (c == 15)
                    s = "o";
                if (c == 16)
                    s = "p";
                if (c == 17)
                    s = "q";
                if (c == 18)
                    s = "r";
                if (c == 19)
                    s = "s";
                if (c == 20)
                    s = "t";
                if (c == 21)
                    s = "u";
                if (c == 22)
                    s = "v";
                if (c == 23)
                    s = "w";
                if (c == 24)
                    s = "x";
                if (c == 25)
                    s = "y";
                if (c == 26)
                    s = "z";
                #endregion

                DrawText(veraSmall, s + ": " + items.Dequeue(),
                    new Point(350, m), Color.White);
            }

            items.Clear();
		}
		static void Draw_Help()
		{
            DrawText(vera, "Help", new Point(400, 20), Color.White);
            DrawText(vera, "Controls:", new Point(10, 50), Color.White);
            DrawText(veraSmall, "Movement: arrows, end, home, pgup, pgdn; numpad", new Point(10, 80), Color.White);

            DrawText(veraSmall, "c - Close", new Point(10, 95), Color.White);
            DrawText(veraSmall, "e - Engrave", new Point(10, 110), Color.White);
            DrawText(veraSmall, "h - Debug digging", new Point(10, 125), Color.White);
            DrawText(veraSmall, "i - Inventory menu", new Point(10, 140), Color.White);
            DrawText(veraSmall, "k - Kick/Dismantle", new Point(10, 155), Color.White);
            DrawText(veraSmall, "l (L) - Toggle debug omnivision", new Point(10, 170), Color.White);
            DrawText(veraSmall, "o - Open", new Point(10, 185), Color.White);
            DrawText(veraSmall, "w - Unwield an item", new Point(10, 200), Color.White);
            DrawText(veraSmall, "W - Remove an item", new Point(10, 215), Color.White);
            DrawText(veraSmall, "x - Examine at range", new Point(10, 230), Color.White);
            DrawText(veraSmall, "X - Debug mode", new Point(10, 245), Color.White);
            DrawText(veraSmall, "z - Status", new Point(10, 260), Color.White);
            DrawText(veraSmall, ", - Pick up item", new Point(10, 275), Color.White);

            DrawText(veraSmall, "Esc (Normal Game) - Main menu", new Point(10, 305), Color.White);
            DrawText(veraSmall, "Space, Escape - Back a menu", new Point(10, 320), Color.White);
		}
		static void Draw_Main()
		{
            Draw_HUD();			
			Draw_Tiles();
            DrawMessageBox(); // putting this on the bottom so the screen doesn't flash so much...
        }
        static void Draw_Inventory()
		{
			Draw_Main(); //Need the background
            //SdlGfx.boxColor(screen, (short)(windowSizeX * 0.66), 0, (short)windowSizeX, (short)windowSizeY,
            //    Color.FromArgb(1, 1, 1, 255).ToArgb()); //Black backdrop
            //SdlGfx.rectangleColor(screen, (short)(windowSizeX * 0.66), 0, (short)windowSizeX, (short)windowSizeY,
            //    Color.White.ToArgb()); //White border
            ClearScreen();
            DrawText(vera, "Inventory Menu", new Point(690, 20), Color.White);
            DrawText(veraSmall, "Cancel: Space", new Point(605, windowSizeY - 20), Color.White);

            #region List items
            if (inventorySelect == 0) //If none selected
            {
                int m = 60;
                Queue<string> items = new Queue<string>();
                foreach (Item item in currentLevel.creatureList[0].inventory)
                {
                    if (items.Count >= 26)
                    {
                        items.Dequeue(); //Don't let there be more than 26 in the queue
                    }

                    items.Enqueue(CapitalizeFirst(item.name));
                }

                int count = items.Count;
                for (int c = 1; c <= count; c++)
                {
                    m += 15; //Skip down 15 pixels

                    #region Get item letter
                    string s = "a";
                    if (c == 2)
                        s = "b";
                    if (c == 3)
                        s = "c";
                    if (c == 4)
                        s = "d";
                    if (c == 5)
                        s = "e";
                    if (c == 6)
                        s = "f";
                    if (c == 7)
                        s = "g";
                    if (c == 8)
                        s = "h";
                    if (c == 9)
                        s = "i";
                    if (c == 10)
                        s = "j";
                    if (c == 11)
                        s = "k";
                    if (c == 12)
                        s = "l";
                    if (c == 13)
                        s = "m";
                    if (c == 14)
                        s = "n";
                    if (c == 15)
                        s = "o";
                    if (c == 16)
                        s = "p";
                    if (c == 17)
                        s = "q";
                    if (c == 18)
                        s = "r";
                    if (c == 19)
                        s = "s";
                    if (c == 20)
                        s = "t";
                    if (c == 21)
                        s = "u";
                    if (c == 22)
                        s = "v";
                    if (c == 23)
                        s = "w";
                    if (c == 24)
                        s = "x";
                    if (c == 25)
                        s = "y";
                    if (c == 26)
                        s = "z";
                    #endregion

                    DrawText(veraSmall, s + ": " + items.Dequeue(),
                        new Point(650, m), Color.White);
                }

                items.Clear();
            }
            #endregion
            else if (inventorySelect <= currentLevel.creatureList[0].inventory.Count) //If the item exists
            {
                if (inventoryMode == 0)
                {
                    DrawText(veraSmall, CapitalizeFirst(currentLevel.creatureList[0].inventory[
                        inventorySelect - 1].name), new Point(650, 75), Color.White); //Draw selected item's name

                    DrawText(veraSmall, " - [b]reak down", new Point(650, 90), Color.White);
                    DrawText(veraSmall, " - [c]ombine craft", new Point(650, 105), Color.White);
                    DrawText(veraSmall, " - [d]rop", new Point(650, 120), Color.White);
                    DrawText(veraSmall, " - [e]at", new Point(650, 135), Color.White);
                    DrawText(veraSmall, " - [f]ire/throw", new Point(650, 150), Color.White);
                    DrawText(veraSmall, " - [u]se", new Point(650, 165), Color.White);
                    DrawText(veraSmall, " - [w]ield", new Point(650, 180), Color.White);
                    DrawText(veraSmall, " - [W]ear", new Point(650, 195), Color.White);
                }
                else if (inventoryMode == 1) //Craft this item menu
                {
                    int m = 60;
                    Queue<string> items = new Queue<string>();
                    foreach (Item item in craftableItems)
                    {
                        if (items.Count >= 24)
                        {
                            items.Dequeue(); //Don't let there be more than 24 in the queue
                        }

                        items.Enqueue(CapitalizeFirst(item.name));
                    }

                    int count = items.Count;
                    if (count <= 0)
                        DrawText(veraSmall, "Nothing occurs to you, given what you have.",
                                               new Point(650, 75), Color.White);

                    for (int c = 1; c <= count; c++)
                    {
                        m += 15; //Skip down 15 pixels

                        #region Get item letter
                        string s = "a";
                        if (c == 2)
                            s = "b";
                        if (c == 3)
                            s = "c";
                        if (c == 4)
                            s = "d";
                        if (c == 5)
                            s = "e";
                        if (c == 6)
                            s = "f";
                        if (c == 7)
                            s = "g";
                        if (c == 8)
                            s = "h";
                        if (c == 9)
                            s = "i";
                        if (c == 10)
                            s = "j";
                        if (c == 11)
                            s = "k";
                        if (c == 12)
                            s = "l";
                        if (c == 13)
                            s = "m";
                        if (c == 14)
                            s = "n";
                        if (c == 15)
                            s = "o";
                        if (c == 16)
                            s = "p";
                        if (c == 17)
                            s = "q";
                        if (c == 18)
                            s = "r";
                        if (c == 19)
                            s = "s";
                        if (c == 20)
                            s = "t";
                        if (c == 21)
                            s = "u";
                        if (c == 22)
                            s = "v";
                        if (c == 23)
                            s = "w";
                        if (c == 24)
                            s = "x";
                        if (c == 25)
                            s = "y";
                        if (c == 26)
                            s = "z";
                        #endregion

                        DrawText(veraSmall, s + ": " + items.Dequeue(),
                            new Point(650, m), Color.White);
                    }
                    items.Clear();
                }
            }
		}
		static void Draw_Health()
		{	
			Draw_Main();
            //SdlGfx.boxColor(screen, (short)(windowSizeX * 0.66), 0, (short)windowSizeX, (short)windowSizeY,
            //    Color.FromArgb(1, 1, 1, 255).ToArgb()); //Black backdrop
            //SdlGfx.rectangleColor(screen, (short)(windowSizeX * 0.66), 0, (short)windowSizeX, (short)windowSizeY,
            //    Color.White.ToArgb()); //White border

            DrawText(vera, "Status Menu", new Point(690, 20), Color.White);
            DrawText(veraSmall, "Cancel: Space", new Point(605, windowSizeY - 20), Color.White);

            #region List parts
            var player = GetPlayer1();
            int partCount = player.anatomy.Count;
            int m = 60;
            Queue<string> parts = new Queue<string>();
            Queue<Color> partDamage = new Queue<Color>();
            float healthRatio = 1f;

            foreach (BodyPart part in currentLevel.creatureList[0].anatomy)
            {
                if (parts.Count >= 24)
                {
                    parts.Dequeue(); //Don't let there be more than 24 in the queue
                    partDamage.Dequeue();
                }

                parts.Enqueue(CapitalizeFirst(part.name));
               
                switch (part.injury)
                {
                    case InjuryLevel.Healthy:
                        partDamage.Enqueue(Color.White);
                        break;
                    case InjuryLevel.Minor:
                        partDamage.Enqueue(Color.Green);
                        break;
                    case InjuryLevel.Broken:
                        partDamage.Enqueue(Color.Yellow);
                        break;
                    case InjuryLevel.Mangled:
                        partDamage.Enqueue(Color.Crimson);
                        break;
                    case InjuryLevel.Destroyed:
                        partDamage.Enqueue(Color.Gray);
                        break;
                    default:
                        throw new Exception($"Unhandled InjuryType '${part.injury.ToString()}' when displaying body parts");
                }

                healthRatio = (float)part.currentHealth / (float)part.noInjury; // This is the health ratio.                        
                if (healthRatio >= 1.00)
                    partDamage.Enqueue(Color.White);
                else if (healthRatio > 0.75)
                    partDamage.Enqueue(Color.Cyan);
                else if (healthRatio > 0.5)
                    partDamage.Enqueue(Color.Green);
                else if (healthRatio > 0.25)
                    partDamage.Enqueue(Color.Yellow);
                else if (healthRatio > 0)
                    partDamage.Enqueue(Color.Crimson);
                else
                    partDamage.Enqueue(Color.Gray);
            }

            partCount = parts.Count;
            for (int c = 1; c <= partCount; c++)
            {
                m += 15; //Skip down 15 pixels
                DrawText(veraSmall, parts.Dequeue(),
                    new Point(650, m), partDamage.Dequeue());
            }

            parts.Clear();
            #endregion
		}
		static void Draw_GetPos()
		{
			Draw_Main();
            //SdlGfx.boxColor(screen, 5, 533, 895, (short)(windowSizeY * 0.992), Color.Black.ToArgb());
            //SdlGfx.rectangleColor(screen, 5, 533, 895, (short)(windowSizeY * 0.992), Color.White.ToArgb());
            
            DrawImage(88, new Point(cursorPos.X * TILEWIDTH, cursorPos.Y * TILEHEIGHT), Color.Yellow, "8"); //Draw Cursor

            #region Description
            int m = 535;
            Queue<string> messages = new Queue<string>();
            Tile thisTile = currentLevel.tileArray[cursorPos.X, cursorPos.Y];

            if (currentLevel.LineOfSight(currentLevel.creatureList[0].pos, cursorPos)) //If it can be seen
            {
                foreach (Creature c in currentLevel.creatureList)
                {
                    if (c.pos == cursorPos) //If creature is at this position
                    {
                        messages.Enqueue("There is a " + c.Stats.name + " here.");
                    }
                }

                if (thisTile.itemList.Count > 0)
                {
                    messages.Enqueue("There is a " + thisTile.itemList[0].name + " here.");
                }

                if (thisTile.fixtureLibrary.Count > 0)
                {
                    if (thisTile.fixtureLibrary[0] is Trap)
                    {
                        Trap t = (Trap)thisTile.fixtureLibrary[0];
                        if (t.visible)
                            messages.Enqueue("There is a " + thisTile.fixtureLibrary[0].type + " here.");
                    }
                    else
                    {
                        messages.Enqueue("There is a " + thisTile.fixtureLibrary[0].type + " here.");
                    }
                }

                if (thisTile.isWall)
                {
                    messages.Enqueue("This is a " + thisTile.material.name + " wall.");
                }
            }
            else
            {
                messages.Enqueue("You cannot see that space at the moment");
            }

            while(messages.Count >= MaxMessages)
            {
                messages.Dequeue(); //Don't let there be more than fourteen in the queue
            }

            if (messages.Count <= 0)
            {
                messages.Enqueue("There is nothing noteworthy here.");
            }

            foreach (string message in messages)
            {
                m += 15; //Skip down 15 pixels
                DrawText(veraSmall, message, new Point(10, m), Color.White);
            }

            messages.Clear();
            #endregion
		}
		static void Draw_Escape()
		{
	        DrawText(vera, "Main Menu", new Point(380, 20), Color.White);
	
	        if (selectionCursor == 1) //If New Game is highlighted
	            DrawText(vera, "<Return to game>", new Point(350, windowSizeY / 2), Color.White);
	        else
	            DrawText(vera, "Return to game", new Point(350, windowSizeY / 2), Color.Gray);
	
	        if (selectionCursor == 2) //If Quit is highlighted
	            DrawText(vera, "<Quit and Save>", new Point(350, windowSizeY / 2 + 30), Color.White);
	        else
	            DrawText(vera, "Quit and Save", new Point(350, windowSizeY / 2 + 30), Color.Gray);
		}

        public const int MaxMessages = 10;
        static void Draw_HUD()
        {
            DrawStatBox();
        }

        private static void DrawMessageBox()
        {
            int m = 520;
            Queue<string> messages = new Queue<string>();
            foreach (string message in currentLevel.creatureList[0].message)
            {
                if (messages.Count >= 14)
                {
                    messages.Dequeue(); //Don't let there be more than fourteen in the queue
                }

                messages.Enqueue(message);
            }

            foreach (string message in messages)
            {
                m += 15; //Skip down 15 pixels
                DrawText(veraSmall, message, new Point(280, m), Color.White);
            }

            messages.Clear();
        }

        
        
        private static void DrawStatBox()
        {
            var player = GetPlayer1();

            FoodLevel currentFoodLevel = FoodLevel.Default;
            foreach(var level in FoodLevel.FoodLevels.Select(pair => pair.Value))
            {
                if (Mathematics.IsWithinRange(player.food, level.Range))
                {
                    currentFoodLevel = level;
                }
            }

            Color injuryColor = Color.White;

            if (currentLevel.creatureList[0].hp >= currentLevel.creatureList[0].hpMax)
                injuryColor = Color.White;
            else if (currentLevel.creatureList[0].hp > currentLevel.creatureList[0].hpMax * 0.75)
                injuryColor = Color.LightCyan;
            else if (currentLevel.creatureList[0].hp > currentLevel.creatureList[0].hpMax * 0.5)
                injuryColor = Color.Green;
            else if (currentLevel.creatureList[0].hp > currentLevel.creatureList[0].hpMax * 0.25)
                injuryColor = Color.Yellow;
            else if (currentLevel.creatureList[0].hp > currentLevel.creatureList[0].hpMax * 0.0)
                injuryColor = Color.FromArgb(255, 50, 50);
            else if (currentLevel.creatureList[0].hp <= 0)
                injuryColor = Color.Gray;

            int displayTurn = (int)totalTurnCount;

            DrawText(veraSmall, "Turn: " + displayTurn.ToString() + "\t",
                new Point(10, 535), Color.White, false); //Write turn count

            DrawText(veraSmall, "HP: " + currentLevel.creatureList[0].hp + "/" + currentLevel.creatureList[0].hpMax + "\t\t",
                new Point(175, 535), injuryColor, false); //Write turn count

            DrawText(veraSmall, "XP: " + currentLevel.creatureList[0].xp + "/" + currentLevel.creatureList[0].xpBorder * 2 + "\t\t",
                new Point(175, 550), Color.White, false); //Write turn count

            DrawText(veraSmall, "GP: " + currentLevel.creatureList[0].gold + "\t",
                new Point(175, 565), Color.White, false); //Write turn count

            DrawText(veraSmall, "Area: (" + mapPos.X + ", " + mapPos.Y + ", " + mapPos.Z + ")\t",
                new Point(10, 550), Color.White, false);

            DrawText(veraSmall, "Hunger: ", new Point(10, 610),
                Color.White, false);

            DrawText(veraSmall, currentFoodLevel.ToString() + "\t", new Point(110, 610), currentFoodLevel.Color);

            DrawText(veraSmall, "STR: " + currentLevel.creatureList[0].strength +
                " DEX: " + currentLevel.creatureList[0].dexterity +
                " CON: " + currentLevel.creatureList[0].constitution +
                " INT: " + currentLevel.creatureList[0].intelligence +
                " WIS: " + currentLevel.creatureList[0].wisdom +
                " CHA: " + currentLevel.creatureList[0].charisma, new Point(10, 730));
        }

        static void Draw_Tiles()
		{
            for (int y = 0; y < Level.GRIDH; y++)
			{
                for (int x = 0; x < Level.GRIDW; x++)
                {
                    DrawGridRow(y, x);                    
                }
                Console.Write("\n");
            }
		}

        private static void DrawGridRow(int y, int x)
        {
            Tile thisTile = currentLevel.tileArray[x, y]; //Shorthand for this tile
            Creature player = currentLevel.creatureList[0]; //Shorthand for the player creature

            ConsoleTile toDraw = ConsoleTile.Blank;
            if (currentLevel.levelType == "forest")
            {
                toDraw.Color = Color.LightGreen; //Grassy forest floor
            }

            if (thisTile.lastSeenTile != null) //If we've seen it
            {
                toDraw = thisTile.lastSeenTile; //Draw the remembered tile
                toDraw.Color = Color.DimGray; //But washed out
            }

            bool LOS = ((currentLevel.LineOfSight(player.pos, new Point(x, y))) || iCanSeeForever);
            if (LOS && player.blind <= 0) //If in line of sight and not blind
            {
                toDraw = DrawLineOfSight(y, x, toDraw);
            }

            toDraw = DrawItemsOnGround(y, x, player, LOS, toDraw);
            toDraw = DrawCreaturesHere(y, x, player, LOS, toDraw); // player is a creature.

            if (Mathematics.IsWithinRange(toDraw.ImageIndex, new Range(0, 256)) &&
                toDraw.Location != player.pos) //If existent
            {
                thisTile.lastSeenTile = toDraw; //Remember image
                //DrawImage(-1, new Point(x * TILEWIDTH, y * TILEHEIGHT), toDraw.Color, "."); //Draw this tile's stuff
            }

            //if (player.pos == new Point(x, y)) //PC should always be drawn
            //{
            //    toDraw.ImageIndex = player.Stats.creatureImage;
            //    toDraw.Color = player.Stats.color;
            //    //DrawImage(5, new Point(x * TILEWIDTH, y * TILEHEIGHT), toDraw.Color, "@"); //Draw this tile's stuff
            //}
            DrawImage(toDraw);

            //int smellTotal = 0;
            //foreach (int s in currentLevel.tileArray[x, y].scentMagnitude)
            //{
            //    smellTotal += s;
            //}
            //DrawText(veraSmall, smellTotal.ToString(), new Point(x * TILEWIDTH, y * TILEHEIGHT), Color.Red);                        
        }

        private static ConsoleTile DrawCreaturesHere(int y, int x, Creature player, bool LOS, ConsoleTile prevTile)
        {
            foreach (Creature c in currentLevel.creatureList)
            {
                var tileToDraw = new ConsoleTile(c.Stats.name, Color.Red);
                if (c.pos == new Point(x, y) && //If there's a creature here
                    ((LOS && player.blind <= 0 && (c.invisibility <= 0 || player.seeInvisible > 0)) ||
                     player.detectMonster > 0)) //And we can see or detect it
                {
                    tileToDraw.ImageIndex = c.Stats.creatureImage;
                    tileToDraw.Color = c.Stats.color;
                    return tileToDraw.DeepClone();
                }
            }
            return prevTile.DeepClone();
        }

        [Serializable]
        public class ConsoleTile
        {
            public static readonly ConsoleTile Blank = new ConsoleTile(".", Color.Black )
            {
                ImageIndex = 46, //Open floor tile                
            };
            public static readonly ConsoleTile Wall = new ConsoleTile("|", Color.Tan)
            {
                ImageIndex = 219, //Open floor tile
            };

            public string Text;
            public Color Color;
            public int ImageIndex = -1;
            public Point Location;
            public Tile Tile;
            public ConsoleTile(string text, Color color)
            {
                Text = text;
                Color = color;
                Tile = new Tile();
            }
            public override string ToString()
            {
                return Color.Name + " " + Text;
            }
            public ConsoleTile DeepClone()
            {
                var clone = new ConsoleTile(Text, Color);
                clone.ImageIndex = ImageIndex;
                clone.Location = new Point(Location.X, Location.Y);
                clone.Tile = Tile;
                return clone;
            }
        }

        private static ConsoleTile DrawItemsOnGround(int y, int x, Creature player, bool LOS, ConsoleTile prevTile)
        {
            var tileToDraw = new ConsoleTile("/", Color.White) {
                Location = new Point(x, y),
                Tile = currentLevel.tileArray[x, y],
            };
            if (tileToDraw.Tile.itemList.Any() && //If there's an item here
               (player.detectItem > 0 || (LOS && player.blind <= 0))) //If detecting items or can see them
            {
                int topIndex = tileToDraw.Tile.itemList.Count - 1;
                var topItem = tileToDraw.Tile.itemList[topIndex];
                if (topItem != null)
                {
                    tileToDraw.ImageIndex = topItem.itemImage; //Top item image
                    tileToDraw.Color = topItem.color;
                    return tileToDraw.DeepClone();
                }
            }
            return prevTile.DeepClone();
        }

        private static ConsoleTile DrawLineOfSight(int y, int x, ConsoleTile prevTile)
        {
            ConsoleTile tileToDraw = ConsoleTile.Blank;
            var thisTile = currentLevel.tileArray[x, y];
            if (thisTile.isWall)
            {
                tileToDraw = ConsoleTile.Wall;
            }
            tileToDraw.Tile = thisTile;
            tileToDraw.Location = new Point(x, y);
            

            // See if it's a door or trap
            if(DrawLOS_Fixture(ref tileToDraw))
            {
                return tileToDraw;
            }

            return prevTile;
        }
        public static bool DrawLOS_Fixture(ref ConsoleTile tileToDraw)
        {
            if (tileToDraw.Tile.fixtureLibrary.Any())
            {
                int topIndex = tileToDraw.Tile.fixtureLibrary.Count - 1;
                var currentFixture = tileToDraw.Tile.fixtureLibrary[topIndex];
                if (currentFixture is Trap)
                {
                    Trap trap = (Trap)currentFixture;
                    if (trap.visible)
                    {
                        tileToDraw.ImageIndex = trap.imageIndex;
                        tileToDraw.Color = trap.color;
                        return true;
                    }
                }
            }
            return false;
        }

        static void ClearScreen()
        {
            Console.Clear();
        }
        static void DrawText(FontFamily fontToDraw, string text, int x, int y, bool newLine = true)
        {
            DrawText(fontToDraw, text, new Point(x, y), Color.White, newLine);
        }
        static void DrawText(FontFamily fontToDraw, string text, Point position, bool newLine = true)
        {
            DrawText(fontToDraw, text, position, Color.White, newLine);
        }
        static void DrawText(FontFamily fontToDraw, string text, Point position, Color color, bool newLine = true)
        {
            if (newLine)
                Console.WriteLine(text);
            else
                Console.Write(text);
        }
        public static void DrawImage(ConsoleTile tile)
        {
            DrawImage(tile.ImageIndex, tile.Location, tile.Color, tile.Text);
        }
        public static void DrawImage(int imageToDraw, Point position, Color color, string str)
        {
            target.x = (short)position.X;
            target.y = (short)position.Y;

            Console.ForegroundColor = color.ToConsoleColor();
            Console.Write(str.Substring(0, 1));
            Console.ResetColor();
            //Sdl.SDL_BlitSurface(image[imageToDraw], ref source, screen, ref target);
        }
	}
}

