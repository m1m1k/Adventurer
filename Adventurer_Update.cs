using System; //General C# functions
using System.Collections.Generic; //So I can use List
using System.Drawing; //The colors, man
using System.IO; //Mainly used for reading and writing files
using System.Linq;
using System.Runtime.InteropServices; //For use in wrangling pointers in their place
using System.Threading;
using Tao.Sdl;
using TimeLords;
using TimeLords.Creature;
using TimeLords.General;

namespace TimeLords
{
	public partial class Adventurer
	{
        #region Variables		
        public static Sdl.SDL_Event keyEvent; //To tell whether a key was pressed or lifted
        public static Sdl.SDL_Event oldKeyEvent; //For telling if a key was just pressed
        public static bool[] keys = new bool[255]; //An array to see which keys are down.
        public static bool shift = false; //Whether the shift key is down
        public static bool[] pressList = new bool[10];
		#endregion
		
		static void Update()
		{
			switch (gameState)
			{
			case GameState.OpeningMenu:
				Update_OpeningMenu();
				break;
				
			case GameState.MainGame:
				Update_MainGame();
				break;
				
			case GameState.EscapeMenu:
				Update_EscapeMenu();
				break;
				
			case GameState.HealthMenu:
				Update_HealthMenu();
				break;
				
			case GameState.HelpMenu:
				Update_HelpMenu();
				break;
				
			case GameState.InventoryMenu:
				Update_InventoryMenu();
				break;
			}
		}
		
        public static void CreateNewGame()
        {
            worldSeed = (int)DateTime.Now.Ticks; //Set the world seed

            gameState = NAME_SELECT; //Yup, name selection
            ClearAndDraw(); //Draw the name select menu

            sessionName = Update_GetUserName();

            if (Directory.Exists("Saves/" + sessionName))
            {
                try
                {
                    FileL_ImportUniverse(sessionName); //Load it if we've got it
                }
                catch(Exception ex)
                {
                    ClearScreen();
                    
                    DrawText(veraSmall, "Loading of {" + sessionName + "} has failed." + ex.Message,
                        new Point(windowSizeX / 2 - 130, windowSizeY / 2));
                    DrawText(veraSmall, "You can either clear it and start a new game, or save it and quit.",
                        new Point(windowSizeX / 2 - 130, windowSizeY / 2 + 15));
                    DrawText(veraSmall, "Delete corrupted data? [y/n]",
                        new Point(windowSizeX / 2 - 130, windowSizeY / 2 + 30));
                    //Sdl.SDL_Flip(screen);

                    if (Update_GetKey() == ConsoleKey.Y)
                    {
                        Directory.Delete("Saves/" + sessionName, true);

                        string[] lines = File.ReadAllLines("Saves/SaveStockpile.txt"); //Read in the list of sessions
                        List<string> newData = new List<string>();

                        for (int n = 0; n < lines.Length; n++) //Loop through the lines
                        {
                            if (lines[n].StartsWith("[NAME] " + sessionName))
                            {
                                n += 2;
                                if (n >= lines.Length)
                                    break;
                            }

                            newData.Add(lines[n]);
                        }
                        File.WriteAllLines("Saves/SaveStockpile.txt", newData.ToArray());

                        NewGame();
                    }
                    else
                    {
                        run = false;
                        return;
                    }
                }
            }
            else
            {
                NewGame();
            }

            gameState = MAIN_GAME;
        }
        private enum GameMenuChoice
        {
            Unknown, // Not used, only Added because previous system was 1 based, not zero based.
            Start_New_Game = 1,
            Help = 2,
            Quit = 3,
            Load_Game = 4
        }
		static void Update_OpeningMenu()
		{
            const int firstItem = 1;
            const int numListItems = 3;
	        switch (Update_GetKey())
	        {
            	#region Directions
            case ConsoleKey.DownArrow: //If down was pressed                
                pressList[2] = true;
                if (selectionCursor < numListItems) //If down was pressed...                   
                    selectionCursor++; //...move the cursor down one, if possible.
                else
                    selectionCursor = firstItem; //Loop over if needed
                break;                

            case ConsoleKey.UpArrow: //If up was pressed
                if (selectionCursor > firstItem)
                    selectionCursor--;
                else
                    selectionCursor = numListItems;
                break;
            	#endregion

            case ConsoleKey.Enter: //If enter aka "271" or "13" was pressed
                    switch ((GameMenuChoice)selectionCursor) //If "New Game"
	                {
                        case GameMenuChoice.Start_New_Game:
                            CreateNewGame();
                            break;
                        case GameMenuChoice.Help:
                            gameState = HELP_MENU;
                            break;
                        case GameMenuChoice.Quit:
                            run = false;
                            break;
                        case GameMenuChoice.Load_Game:
                            LoadGame();
                            break;
                    }
                    break;
            }	                       
		}

        private static void LoadGame()
        {
            string[] saves = Directory.GetDirectories("Saves/");

            if (saves.Length > 0) //If there's a save game
            {
                StreamReader read = new StreamReader(saves[0] + "/WorldData.txt"); //Open up the world data file

                string line = read.ReadLine(); //Read the seed in
                line = line.Remove(0, 7); //Remove "[SEED] "
                worldSeed = int.Parse(line); //Seed the world

                line = read.ReadLine(); //Read in the player position

                read.Dispose();
                read.Close(); //Close the file
                gameState = MAIN_GAME;
            }
        }

        static void Update_MainGame()
		{
            var player = GetPlayer1();
            for (int n = 0; n < currentLevel.creatureList.Count; n++) //For every creature
            {
                UpdateCreatureEnergy(player, currentLevel.creatureList[n]);                
            }

			Update_World();            
		}

        public const int NoEnergyConsumed = -1;
        public static int UpdateCreatureEnergy(Creature player, Creature currentCreature)
        {
            if (currentCreature.status_paralyzed > 0)
            {
                return NoEnergyConsumed;
            }

            // Check to see if it's current creature's turn
            if (currentCreature.turn_energy >= TURN_THRESHOLD) 
            {
                if (currentCreature == player)
                {
                    if (!PlayerActionCompleted(player)) //If skipped turn
                    {
                        return NoEnergyConsumed;
                    }
                }
                else
                {
                    Update_Creature(currentCreature);
                }

                Update_PostTurn(currentCreature);
            }

            //Every cycle, add speed to current energy
            currentCreature.turn_energy += currentCreature.Stats.speed; 
            return 0;
        }
        static void Update_HealthMenu()
		{
            switch (Update_GetKey())
            {
                case ConsoleKey.Escape:
                case ConsoleKey.Spacebar:
                    gameState = MAIN_GAME;
                    break;
            }
		}
		static void Update_HelpMenu()
		{		
			switch(Update_GetKey())
			{
                case ConsoleKey.Escape:
                case ConsoleKey.Spacebar:
                    gameState = OPENING_MENU;				
                return;
			}
		}
		static void Update_EscapeMenu()
		{
            switch (Update_GetKey())
            {
                case ConsoleKey.Escape:
                case ConsoleKey.Spacebar:
                    gameState = MAIN_GAME;
                break;

            case ConsoleKey.DownArrow: //If down was pressed
                if (selectionCursor < 2) //If down was pressed...                   
                    selectionCursor++; //...move the cursor down one, if possible.
                else
                    selectionCursor = 1; //Loop over if needed
                break;

            case ConsoleKey.UpArrow: //If up was pressed
                if (selectionCursor > 1)
                    selectionCursor--;
                else
                    selectionCursor = 2;
                break;

            case ConsoleKey.Enter: //If enter aka "271" was pressed
                if (selectionCursor == 1) //If "Return to game"
                    gameState = MAIN_GAME;
                else if (selectionCursor == 2) //If "Quit and Save"
                {
                    FileS_World();
                    run = false;
                }
                break;
            }
		}
		static void Update_InventoryMenu()
		{
            var player = GetPlayer1();
            if (inventorySelect <= 0) //If at the outer menu
            {
				Inv_Outer();
            }
            else //If within an item
            {
                if (inventorySelect > player.inventory.Count) //If the item is nonexistent
                    inventorySelect = 0; //Revert

                if (inventoryMode == 0) //If we're just looking at an item
                {
                    Inv_Main();
                }
                else if (inventoryMode == 1) //If in crafting mode
                {
                    Inv_CombCraft();
                }

                inventorySelect = 0; //Revert
                inventoryMode = 0;
            }		
		}
		
		static void Inv_Outer()
		{
            var player = GetPlayer1();
            bool done = false;
			
			while(!done)
			{
				var input = Update_GetKey();
                
			    switch (input)
	            {
                    case ConsoleKey.Enter:
                    case ConsoleKey.Spacebar:
                        gameState = MAIN_GAME;
	                    return;
	            }

                var letter = input.ToString();
                var letterNum = LetterIndexToNumber(letter);
                if(Char.IsLetter(letter[0]))
                {
                    inventorySelect = Mathematics.ForceIntoArrayBounds(letterNum, player.inventory);
                }
                if (letterNum == inventorySelect) // If a valid selection
                {
					Inv_Main();
					gameState = MAIN_GAME;
					done = true;
				}
			}
		}
		static void Inv_Main()
		{
            var player = GetPlayer1();
            Item thisItem = player.inventory[inventorySelect - 1];

            ClearAndDraw();
			while (true)
			{
	            switch (Update_GetKey())
	            {
                    case ConsoleKey.Enter:
                    case ConsoleKey.Spacebar:
                    case ConsoleKey.Backspace:
					inventorySelect = 0; //Nothing selected anymore
					ClearAndDraw(); //Update the screen for backing out of menu
					return;
					
	            case ConsoleKey.B: //Break down item
	                player.BreakDownItem(currentLevel, thisItem); //Break down said item
	                inventorySelect = 0;
	                gameState = MAIN_GAME;
	                return;
	
	            case ConsoleKey.C:
	                #region Enter Combine Craft Mode
	                if (player.isDextrous)
	                {
	                    craftableItems = player.FindValidRecipe( //Find list of items
	                        thisItem, currentLevel.itemLibrary); //that can be made	
	                    inventoryMode = 1; //Crafting mode with item
						Inv_CombCraft();
						return;
	                }
	                else
	                {
	                    player.message.Add("Your limbs are too clumsy to make tools.");
	                    inventorySelect = 0; //Back out of menu
	                    gameState = MAIN_GAME;
	                }
	                #endregion
	                return;
	
	            case ConsoleKey.D:
	                player.Drop(currentLevel, thisItem); //Drop said item	
	                inventorySelect = 0; //Back out of menu 
	                gameState = MAIN_GAME;
	                return;
	
	            case ConsoleKey.E:
	                #region Eat Item
	                if (thisItem.name.StartsWith(player.Stats.name)) //Aka, goblin eating goblin corpse
	                {
	                    if (player.food > 2500)
	                    {
	                        player.message.Add("You nauseate yourself - you cannot bring yourself to eat one of your own kind");
	                        break;
	                    }
	                    else
	                    {
	                        player.message.Add("It really is a matter of starvation or cannibalism.");
	                    }
	                }
	
	                player.Eat(currentLevel, thisItem); //Eat said item	
	                inventorySelect = 0; //Back out of menu
	                gameState = MAIN_GAME;
	                #endregion
	                return;
	
	            case ConsoleKey.F:
	                #region Fire Item
	                Point targetPos = Update_GetPosition();
	
	                //currentLevel.tileArray[targetPos.X, targetPos.Y].itemList.Add(firedItem); //It ends up on selected tile
	                //player.inventory.RemoveAt(inventorySelect - 1); //Remove item from inventory                                                
	                player.message.Add("You send the " + thisItem.name + " flying.");
	                
	                for (int i = 0; i < currentLevel.creatureList.Count; i++)
	                {
	                    Creature c = currentLevel.creatureList[i];                                   
	                    if (c.pos == targetPos)
	                    {                                                        
	                        player.RangeAttack(currentLevel, c, thisItem);
	                    }
	                }                                                
	
	                inventorySelect = 0; //Back out of menu 
	                gameState = MAIN_GAME;
	                #endregion
	                return;
	
	            case ConsoleKey.U:
	                Inv_Main_UseItem();
	                return;
					
				case ConsoleKey.W:
	                #region Wield Item
	                Item w;
	
	                w = player.inventory[inventorySelect - 1];
	                if (player.CanWield(w))
	                {
	                    player.Wield(w);
	                    gameState = MAIN_GAME;
	                }
	                else
	                {
	                    player.message.Add("The " + w.name + " slips from your grip");
	                }
	                
	                inventorySelect = 0; //Back out of menu
	                gameState = MAIN_GAME;
	                #endregion
	                return;
	
				case ConsoleKey.L: // was "W" now L for Llevar
	                #region Wear Item
	                if (thisItem is Armor)
	                {
						Armor thisArmor = (Armor)thisItem;
	                    if (player.CanWear(thisArmor))
	                    {
	                        player.Wear(thisArmor);//If it's armor, wear it.
	                        gameState = MAIN_GAME;
	                    }
	                }
					else if (thisItem is Amulet)
					{
						Amulet thisAmulet = (Amulet)thisItem;
						if (player.amulet == null)
						{
							player.Wear(new Amulet(thisAmulet), currentLevel);
						}
						else
						{
							player.inventory.Add(new Amulet(player.amulet)); //Copy amulet
							player.amulet = new Amulet(thisAmulet); //Overwrite amulet
						}
					}
	                else
	                    player.message.Add("That's not armor.");
	
	                inventorySelect = 0; //Back out of menu
	                gameState = MAIN_GAME;
	                #endregion
					return;
	            }
			}
		}
        public static Point AskInputKeyToDirection(Point loc, int numSquares = 1)
        {
            return Keyboard.ConsoleKeyToDirection(loc, Update_GetKey(), numSquares);
        }
        static void Inv_Main_UseItem()
		{
            var player = GetPlayer1();
            var whichItem = player.inventory[inventorySelect - 1];
            if (!player.isDextrous) //Check to see if these can be used
            {
                player.message.Add("Your limbs are too clumsy to use this");
            }
            else if (whichItem.use.Count > 1) //If it has multple uses
            {
                //inventoryMode = 2; //Switch to Use mode with item WIP
            }
            else if (whichItem.use.Count == 1)
            {
                //Do the use for the item
                if (whichItem.use[0] == "dig")
                {
                    Use_DiggingTool(player);
                }
                else if (whichItem.use[0] == "mine")
                {
                    Use_MiningTool(player);
                }
                else if (whichItem.use[0] == "tripwire")
                {
                    Use_TripWire(player);
                }
                else if (whichItem.use[0] == "chop")
                {
                    Use_ChoppingTool(player);
                }
            }
            else
            {
                player.message.Add("You can't think of an obvious use for it at the moment.");
            }
            gameState = MAIN_GAME;
            inventorySelect = 0;
            inventoryMode = 0;
		}

        private static void Use_ChoppingTool(Creature player)
        {
            player.message.Add("Choose a direction to chop.");
            gameState = MAIN_GAME;
            var input = Update_GetKey();

            Point playerPos = player.pos;
            Point newPosition = AskInputKeyToDirection(playerPos, 1);
            if (newPosition == Keyboard.Cancelled)
            {
                player.message.Add("Chop cancelled");
                return;
            }

            bool creatureThere = false;
            foreach (Creature c in currentLevel.creatureList)
            {
                if (c.pos == newPosition) //If a creature is there.
                {
                    creatureThere = true;
                }
            }

            if (creatureThere) //Stupid foreach limitations
            {
                player.MeleeAttack(currentLevel, newPosition);
            }
            else
            {
                if (currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary.Count > 0)
                {
                    if (currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary[0] is Tree)
                    {
                        Tree thisTree = (Tree)currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary[0];
                        currentLevel.tileArray[newPosition.X, newPosition.Y].itemList.Add(new Item(CapitalizeFirst(thisTree.species) + " log", Color.Brown));
                        player.message.Add("You cut down the " + thisTree.species + " tree.");
                        currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary.RemoveAt(0);
                    }
                    else if (currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary[0] is Door)
                    {
                        Item stick = new Item("stick", Color.Brown);

                        currentLevel.tileArray[newPosition.X, newPosition.Y].itemList.Add(new Item(stick));
                        currentLevel.tileArray[newPosition.X, newPosition.Y].itemList.Add(new Item(stick));
                        currentLevel.tileArray[newPosition.X, newPosition.Y].itemList.Add(new Item(stick));

                        currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary.RemoveAt(0);
                        currentLevel.tileArray[newPosition.X, newPosition.Y].isPassable = true;
                        currentLevel.tileArray[newPosition.X, newPosition.Y].isTransparent = true;
                        currentLevel.tileArray[newPosition.X, newPosition.Y].isDoor = false;

                        player.message.Add("You chop the door to pieces");
                    }
                }
            }
        }
        private static void Use_TripWire(Creature player)
        {
            Point playerPos = currentLevel.creatureList[0].pos;
            if (currentLevel.tileArray[playerPos.X, playerPos.Y].fixtureLibrary.Count <= 0)
            {
                Trap thisTrap = new Trap(new Effect(rngDie.Roll(5), "tripwire"));
                thisTrap.visible = true; //The player made it, so they can see it
                currentLevel.tileArray[playerPos.X, playerPos.Y].fixtureLibrary.Add(thisTrap);

                player.inventory.RemoveAt(inventorySelect - 1); //Rope is now in trap
                player.message.Add("You make a tripwire from the rope.");
            }
            else
            {
                player.message.Add("There is already a " +
                    currentLevel.tileArray[playerPos.X, playerPos.Y].fixtureLibrary[0].type + " here.");
            }
        }

        private static void Use_MiningTool(Creature player)
        {
            player.message.Add("Choose a direction to dig.");
            gameState = MAIN_GAME;

            var radius = AskInputKeyToDirection(player.pos);
            if (radius == Keyboard.Cancelled)
            {
                return;
            }

            if (currentLevel.tileArray[radius.X, radius.Y].fixtureLibrary.Count > 0)
            {
                if (currentLevel.tileArray[radius.X, radius.Y].fixtureLibrary[0] is Door)
                {
                    if (rngDie.Roll(2) == 1) // 1/2 chance
                    {
                        player.message.Add("Your swing bounces wildly off the door");
                    }
                    else
                    {
                        player.message.Add("You break right through the door");
                        currentLevel.tileArray[radius.X, radius.Y].MakeOpen(); //Clear out adjacent tile
                    }
                }
                else if (currentLevel.tileArray[radius.X, radius.Y].fixtureLibrary[0] is Tree)
                {
                    if (rngDie.Roll(100) == 1) // 1% chance
                    {
                        player.message.Add("You somehow hit a weak point and topple the tree!");
                        Tree thisTree = (Tree)currentLevel.tileArray[radius.X, radius.Y].fixtureLibrary[0];
                        currentLevel.tileArray[radius.X, radius.Y].itemList.Add(new Item(CapitalizeFirst(thisTree.species) + " log", Color.Brown));
                        player.message.Add("You cut down the " + thisTree.species + " tree.");
                        currentLevel.tileArray[radius.X, radius.Y].fixtureLibrary.RemoveAt(0);
                    }
                    else // 99% chance
                    {
                        player.message.Add("You chip off only sawdust");
                        currentLevel.tileArray[radius.X, radius.Y].MakeOpen(); //Clear out adjacent tile
                    }
                }
                else
                {
                    currentLevel.tileArray[radius.X, radius.Y].MakeOpen(); //Clear out adjacent tile
                }
            }
            else
            {
                currentLevel.tileArray[radius.X, radius.Y].MakeOpen(); //Clear out the adjacent tile
            }
        }
        private static void Use_DiggingTool(Creature player)
        {
            Point pos = player.pos;

            if (currentLevel.tileArray[pos.X, pos.Y].hasBeenDug) //If it's been dug
            {
                player.message.Add("You break through the floor, and fall all the way to the next level down.");
                player.TakeDamage(5);
                player.message.Add("You land with a painful thud.");
                mapPos.Z++;
                GenLevel("dungeon", true);
                while (!currentLevel.tileArray[player.pos.X, player.pos.Y].isPassable) //Keep going until we can move
                {
                    player.pos.X = (short)rng.Next(1, Level.GRIDW);
                    player.pos.Y = (short)rng.Next(1, Level.GRIDH);
                }
                Item dirtChunk = new Item("dirt chunk", Color.FromArgb(157, 144, 118)); //Chunk of dirt
                currentLevel.tileArray[player.pos.X, player.pos.Y].itemList.Add(new Item(dirtChunk)); //Put the dirt chunk there
            }
            else
            {
                Item dirtChunk = new Item("dirt chunk", Color.FromArgb(157, 144, 118)); //Chunk of dirt
                currentLevel.tileArray[pos.X, pos.Y].itemList.Add(new Item(dirtChunk)); //Put the dirt chunk there
                currentLevel.tileArray[pos.X, pos.Y].hasBeenDug = true; //We've dug a pit here.
                player.message.Add("You dig a hole, unearthing some dirt.");
            }
        }

        public static bool IsEscapeKey(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.Escape:
                case ConsoleKey.Spacebar:
                case ConsoleKey.Backspace:
                    return true;
                default:
                    return false;
            }
        }
        public static Creature GetPlayer1()
        {
            return currentLevel.creatureList[0];
        }
		static void Inv_CombCraft()
		{
            var player = GetPlayer1();
            ClearAndDraw();
            while(true)
			{
				var input = Update_GetKey();
                if(IsEscapeKey(input))
				{
					gameState = MAIN_GAME;
					inventoryMode = 0;
					inventorySelect = 0;
					return;
				}
				int selection = LetterIndexToNumber(input.ToString());
				
	            Queue<Item> items = new Queue<Item>();
	            foreach (Item item in craftableItems) //Get a queue of all the craftable items
	            {
	                items.Enqueue(item);
	
	                if (items.Count >= 24)
	                {
	                    items.Dequeue(); //Don't let there be more than 24 in the queue
	                }
	            }
	
	            int count = items.Count;
	            for (int c = 1; c <= count; c++) //Pretty much a foreach, but allows deletion
	            {
	                if (c == selection) //If we've selected this item
	                {
	                    player.CombineItem(player.inventory[inventorySelect - 1],
	                        items.Dequeue());
	
	                    inventorySelect = 0; //Back out of menu
	                    inventoryMode = 0;
	                    gameState = MAIN_GAME;
						return;
	                }
	                else
	                {
	                    items.Dequeue();
	                }
	            }
	            items.Clear();
			}
		}
		static void Inv_Use()
		{
			
		}

        /// <summary>
        /// Returns if the turn was spent or not.
        /// </summary>
        /// <returns>if the turn was spent or not, failed actions return false, success actions and movement returns true</returns>
        static bool PlayerActionCompleted(Creature player)
        {
            if (currentLevel.playerDiedHere)
            {
                if (player.revive > 0) //If extra life
                {
                    Player_Revive(player);
                }
                else
                {
                    return Player_Death(player);
                }
            }

            PlayerInputLoop();

            return true;
        } //Returns whether the turn was spent

        private static void PlayerInputLoop()
        {
            bool wait = true;
            while (wait)
            {
                ClearAndDraw();
                ProcessKeyboardInput(Update_GetKey());
                wait = true;
            }
        }

        private static bool Player_Death(Creature player)
        {
            ClearScreen();

            player.message.Add("You have died from " + currentLevel.causeOfDeath);
            player.message.Add("This happened because " + currentLevel.mannerOfDeath);
            player.message.Add("You have slain " + player.killCount + " foes");
            player.message.Add("You lived to see " + exploredLevels + " areas in your world");
            
            while (Update_GetKey() != ConsoleKey.Enter) { }; //Wait for Enter

            FileSaveDeath_World();

            run = false;
            gameState = OPENING_MENU;
            ClearAndDraw();
            return false;
        }

        private static void Player_Revive(Creature player)
        {
            player.revive = 0; //Not anymore, sukka
            if (player.amulet.effect.type == "revive")
            {
                player.message.Add("Your " + player.amulet.name + " disintegrates.");
                player.amulet = null;
            }
            player.hp = player.hpMax; //Full heal
            player.food = 15000; //Full food
            player.message.Add("You have a near death experience");
            player.constitution--;
            currentLevel.playerDiedHere = false;
        }

        public static bool isDirectionKey(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                case ConsoleKey.D4:
                // no 5
                case ConsoleKey.D6:
                case ConsoleKey.D7:
                case ConsoleKey.D8:
                case ConsoleKey.D9:
                case ConsoleKey.NumPad1:
                case ConsoleKey.NumPad2:
                case ConsoleKey.NumPad3:
                case ConsoleKey.NumPad4:
                case ConsoleKey.NumPad6:
                case ConsoleKey.NumPad7:
                case ConsoleKey.NumPad8:
                case ConsoleKey.NumPad9:
                    return true;
                default:
                    return false;
            }
        }
        private static bool Command_Dig(Creature player, Point radius)
        {
            if (debugMode)
            {
                player.message.Add("Choose a direction to dig.");

                radius = AskInputKeyToDirection(player.pos, 5);
                if (radius == Keyboard.Cancelled)
                {
                    player.message.Add("Dig cancelled");
                    return false;
                }

                player.message.Add("You release a blast of unnatural energy, tearing through the walls of the dungeon.");
                currentLevel.DigLine(player.pos, radius);                
            }
            return true;
        }
        /// <summary>
        /// Handles Keyboard Input
        /// Returns true/false to wait for another command.
        /// </summary>
        public static bool ProcessKeyboardInput(ConsoleKey input)
        {
            Point radius = new Point();
            Creature player = GetPlayer1();
            bool wait = true;
            if(isDirectionKey(input))
            {
                return Update_Move(player, input);
            }
            switch (input)
            {
                // WAITing key
                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                case ConsoleKey.Decimal:
                case ConsoleKey.S:
                    return true; //Wait a turn

                case ConsoleKey.PageDown:
                    Command_DescendLevel(ref player, ref wait);
                    return false;

                case ConsoleKey.PageUp:
                    player = Command_AscendLevel(player);
                    return false;

                case ConsoleKey.Escape:
                    gameState = ESCAPE_MENU;
                    return false;

                case ConsoleKey.OemComma:
                    wait = Command_PickUp(player);
                    return false;

                case ConsoleKey.C:
                    Point[] position = Command_Close(player);
                    return false;

                case ConsoleKey.E:
                    Command_Engrave(player);
                    return false;

                case ConsoleKey.H:
                    return Command_Dig(player, radius);

                case ConsoleKey.I:
                    gameState = INVENTORY_MENU; //Gamestate is now the Inventory Menu
                    return false;

                case ConsoleKey.K:
                    return Command_Kick(player);

                case ConsoleKey.L:
                    Command_ToggleInfiniteSight();
                    return false;

                case ConsoleKey.O:
                    position = Command_Open(player);
                    return false;

                case ConsoleKey.V:
                    return Command_Dive(player);

                case ConsoleKey.W:
                    return Command_Weild(player);

                case ConsoleKey.Home:
                    return Command_EquipArmor(player);

                case ConsoleKey.X:
                    Update_GetPosition(); //Retrieve a position
                    return false;

                case ConsoleKey.Tab:
                    Command_ToggleDebugMode(player);
                    return false;

                case ConsoleKey.Z:
                    gameState = HEALTH_MENU;
                    return false;

                default:
                    return false; //If no other appropriate key is pressed, don't use a turn
            }
        }

        private static void Command_ToggleInfiniteSight()
        {
            if (debugMode)
            {
                if (iCanSeeForever)
                    iCanSeeForever = false;
                else
                    iCanSeeForever = true;
            }
        }

        private static void Command_ToggleDebugMode(Creature player)
        {
            if (debugMode) //Toggle debug mode
            {
                debugMode = false;
                player.message.Add("You feel the unnatural energy fade.");
            }
            else
            {
                debugMode = true;
                player.message.Add("You call upon the power of Kalasen.");
            }

            iCanSeeForever = false; //Disable infinisight
        }

        private static bool Command_EquipArmor(Creature player)
        {
            if (player.wornArmor.Count > 0)
            {
                player.RemoveAll();
                return true;
            }
            else
            {
                player.message.Add("You are wearing no armor.");
            }
            return false;
        }

        private static bool Command_Weild(Creature player)
        {
            if (player.weapon == null)
            {
                // TBD : grab the first weapon in your inventory perhaps?
                player.message.Add("You are wielding nothing.");
            }
            else
            {
                player.Unwield();
                return false;
            }

            return true;
        }

        private static bool Command_Dive(Creature player)
        {
            if (debugMode)
            {
                player = player.DeepClone(); //Break reference link --WHY WHY TBD TODO FIX THIS
                player.message.Add("You warp through the floor");
                mapPos.Z++;//Go down a level
                GenLevel("dungeon", true);
                player.message.Add("Now entering area (" + mapPos.X + ", " + mapPos.Y + ", " + mapPos.Z + ")");
            }
            return false;
        }

        private static Point[] Command_Open(Creature player)
        {
            Point[] position = TilesSurroundingPlayer(player);

            for (int dir = 1; dir <= position.Length-1; dir++)
            {
                if (dir == 5)
                {
                    dir++; //Skip 5
                }
                foreach (Fixture f in currentLevel.tileArray[(int)position[dir].X, (int)position[dir].Y].fixtureLibrary)
                {
                    if (f.type == "door")
                    {
                        Door door = (Door)f;
                        if (!door.isOpen)
                        {
                            player.OpenDoor(currentLevel.tileArray[(int)position[dir].X, (int)position[dir].Y], currentLevel);
                        }
                    }
                }
            }

            return position;
        }

        private static Point[] TilesSurroundingPlayer(Creature player)
        {
            Point[] position = new Point[10];
            position[1] = new Point((int)player.pos.X - 1, (int)player.pos.Y + 1);
            position[2] = new Point((int)player.pos.X, (int)player.pos.Y + 1);
            position[3] = new Point((int)player.pos.X + 1, (int)player.pos.Y + 1);
            position[4] = new Point((int)player.pos.X - 1, (int)player.pos.Y);
            position[6] = new Point((int)player.pos.X + 1, (int)player.pos.Y);
            position[7] = new Point((int)player.pos.X - 1, (int)player.pos.Y - 1);
            position[8] = new Point((int)player.pos.X, (int)player.pos.Y - 1);
            position[9] = new Point((int)player.pos.X + 1, (int)player.pos.Y - 1);
            return position;
        }

        private static void Command_Engrave(Creature player)
        {
            currentLevel.tileArray[player.pos.X, player.pos.Y].engraving = Update_GetString((returnString) =>
            {
                //Update the single message
                player.message.Add("~" + returnString);
                return returnString;
            }); //Engrave
        }

        private static Point[] Command_Close(Creature player)
        {
            Point[] position = new Point[10];
            position[1] = new Point((int)player.pos.X - 1, (int)player.pos.Y + 1);
            position[2] = new Point((int)player.pos.X, (int)player.pos.Y + 1);
            position[3] = new Point((int)player.pos.X + 1, (int)player.pos.Y + 1);
            position[4] = new Point((int)player.pos.X - 1, (int)player.pos.Y);
            position[6] = new Point((int)player.pos.X + 1, (int)player.pos.Y);
            position[7] = new Point((int)player.pos.X - 1, (int)player.pos.Y - 1);
            position[8] = new Point((int)player.pos.X, (int)player.pos.Y - 1);
            position[9] = new Point((int)player.pos.X + 1, (int)player.pos.Y - 1);

            for (int dir = 1; dir <= 9; dir++)
            {
                if (dir == 5)
                    dir++; //Skip 5
                foreach (Fixture f in currentLevel.tileArray[(int)position[dir].X, (int)position[dir].Y].fixtureLibrary)
                {
                    if (f.type == "door")
                    {
                        Door door = (Door)f;
                        if (door.isOpen)
                        {
                            player.CloseDoor(currentLevel.tileArray[
                                (int)position[dir].X, (int)position[dir].Y], currentLevel);
                        }
                    }
                }
            }

            return position;
        }
        private static bool Command_Kick(Creature player)
        {
            player.message.Add("Choose a direction.");

            Point newPosition = AskInputKeyToDirection(player.pos, 1);
            if (newPosition == Keyboard.Cancelled)
            {
                player.message.Add("Cancelled");
                return false;
            }

            bool creatureThere = false;

            foreach (Creature d in currentLevel.creatureList)
            {
                if (d.pos == newPosition) //If a creature is there.
                {
                    creatureThere = true;
                }
            }

            if (creatureThere) //Stupid foreach limitations
            {
                player.MeleeAttack(currentLevel, newPosition);
            }
            else if (currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary.Count > 0)
            {
                if (currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary[0] is Door)
                {
                    if (rngDie.Roll(2) == 1) // 1/2 chance
                    {
                        Item stick = new Item("stick", Color.Brown);

                        currentLevel.tileArray[newPosition.X, newPosition.Y].itemList.Add(new Item(stick));
                        currentLevel.tileArray[newPosition.X, newPosition.Y].itemList.Add(new Item(stick));
                        currentLevel.tileArray[newPosition.X, newPosition.Y].itemList.Add(new Item(stick));

                        currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary.RemoveAt(0);
                        currentLevel.tileArray[newPosition.X, newPosition.Y].isPassable = true;
                        currentLevel.tileArray[newPosition.X, newPosition.Y].isTransparent = true;
                        currentLevel.tileArray[newPosition.X, newPosition.Y].isDoor = false;

                        player.message.Add("The door splinters apart.");
                    }
                    else
                    {
                        player.message.Add("The door thuds.");
                    }
                }
                else if (currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary[0] is Trap)
                {
                    Trap thisTrap = (Trap)currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary[0];

                    if (thisTrap.effect.type == "tripwire")
                    {
                        Item rope = new Item("rope", Color.Wheat);

                        foreach (Item t in itemLibrary)
                        {
                            if (t.name == "rope")
                            {
                                rope = new Item(t); //Copy an actual rope if possible
                            }
                        }

                        currentLevel.tileArray[newPosition.X, newPosition.Y].itemList.Add(new Item(rope));

                        player.message.Add("You take apart the tripwire.");

                        currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary.RemoveAt(0);
                    }
                }
                else if (currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary[0] is Stairs)
                {
                    player.TakeDamage(1); //Ow.
                    player.message.Add("You kick the hard stairs and hurt your leg.");
                }
                else if (currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary[0] is Tree)
                {
                    Tree thisTree = (Tree)currentLevel.tileArray[newPosition.X, newPosition.Y].fixtureLibrary[0];

                    if (rng.Next(1, 101) > 50) //50% chance
                    {
                        player.TakeDamage(1); //Ow.
                        player.message.Add("You hurt your leg kicking the tree.");
                    }
                    else if (thisTree.fruit != String.Empty && rng.Next(1, 101) > 50) //If it has fruit, 50% chance
                    {
                        currentLevel.tileArray[player.pos.X, player.pos.Y].itemList.Add(new Item(thisTree.fruit, Color.Lime)); //Add a fruit
                        player.message.Add("A " + thisTree.fruit + " drops at your feet.");
                        thisTree.fruit = String.Empty; //No more fruit
                    }
                }
            }
            else
            {
                player.message.Add("You kick at nothing.");
            }
            return true;
        }
        private static bool Command_PickUp(Creature player)
        {
            bool wait;
            player.PickUp(currentLevel);

            while (player.inventory.Count > 25)
            {
                player.message.Add("You drop your " + player.inventory[25] + " to make room.");
                player.Drop(currentLevel, player.inventory[25]); //Drop item
            }

            wait = false;
            return wait;
        }

        private static Creature Command_AscendLevel(Creature player)
        {
            if (currentLevel.tileArray[player.pos.X, player.pos.Y].fixtureLibrary.Count > 0)
            {
                if (currentLevel.tileArray[player.pos.X, player.pos.Y].fixtureLibrary[0] is Stairs)
                {
                    Stairs stairs = (Stairs)currentLevel.tileArray[player.pos.X, player.pos.Y].fixtureLibrary[0]; //Stairs
                    if (!stairs.isDown)
                    {
                        mapPos.Z--; //Go up a level
                        player = player.DeepClone(); //Break reference link --WHY WHY TBD TODO FIX THIS
                        if (mapPos.Z < 1)
                        {
                            Random thisLevelRNG = new Random(levelSeed[mapPos.X, mapPos.Y, mapPos.Z]); //This level's generator
                            if (thisLevelRNG.Next(0, 100) < 20)
                            {
                                GenLevel("village", true);
                            }
                            else
                            {
                                GenLevel("forest", true);
                            }
                        }
                        else
                            GenLevel("dungeon", true);
                        for (int y = 0; y < Level.GRIDH; y++)
                            for (int x = 0; x < Level.GRIDW; x++)
                                if (currentLevel.tileArray[x, y].fixtureLibrary.Count > 0)
                                    if (currentLevel.tileArray[x, y].fixtureLibrary[0].type == "stairs")
                                    {
                                        stairs = (Stairs)currentLevel.tileArray[x, y].fixtureLibrary[0];
                                        if (stairs.isDown)
                                            player.pos = new Point(x, y); //Place on down stairs
                                    }
                    }
                    else
                    {
                        player.message.Add("How does one go up down stairs?");
                    }
                }
            }

            return player;
        }

        private static void Command_DescendLevel(ref Creature player, ref bool wait)
        {
            if (currentLevel.tileArray[player.pos.X, player.pos.Y].fixtureLibrary.Count > 0)
            { //If there's a fixture here
                if (currentLevel.tileArray[player.pos.X, player.pos.Y].fixtureLibrary[0] is Stairs)
                { //If that fixture is stairs
                    Stairs stairs = (Stairs)currentLevel.tileArray[player.pos.X, player.pos.Y].fixtureLibrary[0]; //Stairs
                    if (stairs.isDown)
                    { //If those stairs are down stairs
                        mapPos.Z++; //Go down a level
                        player = player.DeepClone(); //Break reference link // TBD TODO FIX WTF WTF WTF???!?!?!?!?
                        GenLevel("dungeon", true);
                        player.pos = player.pos; //Place the player at the new up stairs position
                        player = player;
                        wait = true;
                    }
                    else
                    {
                        player.message.Add("How does one go down up stairs?");
                    }
                }
            }
        }
        

        static void Update_Creature(Creature c)
        {
            c.message.Clear(); //Monsters shouldn't need to keep messages
            var action = c.mind.DecideAction(currentLevel, c); //Decide action

            // First they move...
            c.Move_Random(currentLevel);

            #region Item Management Actions

            switch (action.Type)
            {
                case CreatureActionType.Pick_Up:
                    currentLevel = c.PickUp(currentLevel);
                    return;
                case CreatureActionType.Unwield:
                    c.Unwield();
                    return;
                case CreatureActionType.Remove:
                    c.RemoveAll();
                    return;
            }

            if (!c.inventory.Any() || !action.Inputs.Any())
            {
                return;
            }
            var firstInput = action.Inputs.First();
            var item = c.inventory[firstInput];
            // Below are actions requiring having something in your inventory, and having an input number.
            switch (action.Type)
            {
                case CreatureActionType.Wield:                    
                    c.Wield(item);
                    return;
                case CreatureActionType.Wear:
                    c.Wear((Armor)item);
                    return;
                case CreatureActionType.Eat:
                    c.Eat(currentLevel, item);
                    return;
                case CreatureActionType.Attack:
                    c.MeleeAttack(currentLevel, 
                        Keyboard.DirectionNumToPoint(firstInput, c.pos));
                    return;
            }
            #endregion
            
        }



        public const int SpawnTurns = 42;
        static int ticks = 0;
        static void Update_World()
		{
            ticks++;
            if (ticks % (TURN_THRESHOLD / 12) == 0) //If we're in time with the average turn
            {
				ticks = 0;
                totalTurnCount++;

                foreach (Creature c in currentLevel.creatureList)
                {
                    c.CycleWithWorld(currentLevel);
                }
                if ((int)totalTurnCount % SpawnTurns == 0) //Every 42 turns
                {
                    //With a percentage inversely proportional to already existing creatures
                    if (rng.Next(1, currentLevel.creatureList.Count) == 1 && currentLevel.levelType != "village") 
                    {
                        currentLevel.SpawnCreature(false, "monster"); //Spawn new creature
                    }
                }
			}
		}
		static void Update_PostTurn(Creature c)
		{
			c.Wait(currentLevel);
			
            if (c.message.Count > 50)
                c.message.RemoveRange(0, c.message.Count - 50); //Toss excess messages

            #region Check if a creature should be dead
            int killedIndex = 0;

            if (c.ShouldBeDead(currentLevel))
            {
				currentLevel.SlayCreature(c);
            }
            
            if (killedIndex > 0)
                currentLevel.creatureList.RemoveAt(killedIndex);
            #endregion
		}
		static void Update_Hunger(Creature c)
		{
            c.food -= 2; //Hunger

            if (c.food < 0) //If starving
            {
                c.hp--;
                currentLevel.causeOfDeath = "organ failure.";
                currentLevel.mannerOfDeath = "you were starving.";
            }
		}
		static void Update_Smell(Creature c)
		{
            int x = (int)c.pos.X;
            int y = (int)c.pos.Y;
            string smelledWhat = String.Empty;
            for (int i = 0; i < currentLevel.tileArray[x, y].scentMagnitude.Count; i++)
                if (currentLevel.tileArray[x, y].scentMagnitude[i] > c.senseOfSmell)
                {
                    smelledWhat = currentLevel.tileArray[x, y].scentIdentifier[i];
                }
            if (smelledWhat != String.Empty)
            {
                bool seeSmelled = false;
                bool ownSmell = false;
                for (int i = 1; i < currentLevel.creatureList.Count; i++)
                {
                    if (currentLevel.LineOfSight(c.pos,
                        currentLevel.creatureList[i].pos) && currentLevel.creatureList[i].Stats.name == smelledWhat)
                    {
                        seeSmelled = true; //It's the same as something seen
                    }

                    if (smelledWhat == c.Stats.name)
                        ownSmell = true; //It's the creature's own smell                    
                }

                if (!seeSmelled && !ownSmell) //If it's an un-obvious smell
                {
                    int count = c.message.Count;
                    if (count > 0)
                    {
                        //if (currentLevel.creatureList[n].message[count - 1] != "You smell a " + smelledWhat + ".") //Don't spam this
                        //    currentLevel.creatureList[n].message.Add("You smell a " + smelledWhat + ".");
                    }
                }
            }
		}
		static bool Update_Move(Creature c, ConsoleKey key)
		{
            bool peacefulAdjacent = false; //Whether the adjacent creature, if any, is peaceful
            var newPosition = Keyboard.ConsoleKeyToDirection(c.pos, key);
            foreach (Creature d in currentLevel.creatureList)
            {                
                if (d is QuestGiver && d.pos == newPosition)
                {
                    QuestGiver qG = (QuestGiver)d;
                    bool haveItem = false;
                    peacefulAdjacent = true;
                    c.message.Add("You chat with the " + d.Stats.name + ".");

                    int inventoryCount = c.inventory.Count; //Bluh foreach
                    for (int itemIndex = 0; itemIndex < inventoryCount; itemIndex++)
                    {
                        Item item = c.inventory[itemIndex]; //There, foreach simulated
                        if (item.name == qG.wantObject)
                        {
                            haveItem = true;
                            c.message.Add(CapitalizeFirst(d.Stats.name) + ": I see you have a " + qG.wantObject + ". Trade for a " + qG.giveObject + "? (y/n)");
                            ClearAndDraw(); //Draw this to the screen
                            if (Update_GetKey() == ConsoleKey.Y)
                            {
                                d.inventory.Add(item); //Give away the wanted item
                                c.inventory.Remove(item);

                                int cInventoryCount = c.inventory.Count; //Bluh foreach
                                for (itemIndex = 0; itemIndex < cInventoryCount; itemIndex++)
                                {
                                    Item cItem = d.inventory[itemIndex];
                                    c.inventory.Add(cItem); //Recieve giveObject
                                    d.inventory.Remove(cItem);
                                }
                                foreach (Item cItem in d.inventory)
                                {
                                    if (cItem.name == qG.giveObject)
                                    {
                                        c.inventory.Add(cItem);
                                    }
                                }

                                qG.CycleWantGiveItem(itemLibrary); //Cycle what s/he wants and what he will give

                                c.message.Add(CapitalizeFirst(d.Stats.name) + ": You trade your items.");
                            }
                            else
                            {
                                c.message.Add(CapitalizeFirst(d.Stats.name) + ": Too bad. Come back if you change your mind.");
                            }
                            break;
                        }
                    }

                    if (!haveItem) //If we don't have the item
                    {
                        c.message.Add(CapitalizeFirst(d.Stats.name) + ": Hello adventurer. If you bring me a " + qG.wantObject + ", I'll give you a " + qG.giveObject + ".");
                    }
					
					return true; //Talking is not a free action
                }
            }

            if (!peacefulAdjacent)
            {            
	            if (c.CanAttackMelee(currentLevel, newPosition) && !peacefulAdjacent)
				{
	                currentLevel = c.MeleeAttack(currentLevel, newPosition);
					return true;
				}
	            else if (!currentLevel.MoveWillBeBlocked(newPosition))
	            {
	                bool moved = c.Move(currentLevel, newPosition);
					Update_MapEdge(c); //Check for hitting map edge
					return moved;
	            }
			}
			
			return false;
		} //Returns whether a turn was spent
		static void Update_MapEdge(Creature c)
		{
            if (c.pos.X <= 1)
            {
                Creature player = c.DeepClone(); //Grab copy of player
                mapPos.X--; //We've gone to the left
                if (mapPos.X == -1)
                {
                    mapPos.X = 99; //Loop around
                }

                Random thisLevelRNG = new Random(levelSeed[mapPos.X, mapPos.Y, mapPos.Z]); //This level's generator
                if (thisLevelRNG.Next(0, 100) < 30)
                {
                    GenLevel("village", true);
                }
                else
                {
                    GenLevel("forest", true);
                }

                player.pos.X = Level.GRIDW - 2; //Now on other end of map
                player = player; //Creature 0 is the player                                                                        
            }
            else if (c.pos.X >= Level.GRIDW - 1)
            {
                Creature player = c.DeepClone(); //Grab copy of player
                mapPos.X++; //We've gone to the left
                if (mapPos.X == 100)
                {
                    mapPos.X = 0; //Loop around
                }

                Random thisLevelRNG = new Random(levelSeed[mapPos.X, mapPos.Y, mapPos.Z]); //This level's generator
                if (thisLevelRNG.Next(0, 100) < 30)
                {
                    GenLevel("village", true);
                }
                else
                {
                    GenLevel("forest", true);
                }
                player.pos.X = 2; //Now on other end of map
                player = player; //Creature 0 is the player                                    
            }
            else if (c.pos.Y <= 1)
            {
                Creature player = c.DeepClone(); //Grab copy of player
                mapPos.Y--; //We've gone to the left
                if (mapPos.Y == -1)
                {
                    mapPos.Y = 99; //Loop around
                }

                Random thisLevelRNG = new Random(levelSeed[mapPos.X, mapPos.Y, mapPos.Z]); //This level's generator
                if (thisLevelRNG.Next(0, 100) < 30)
                {
                    GenLevel("village", true);
                }
                else
                {
                    GenLevel("forest", true);
                }

                player.pos.Y = Level.GRIDH - 2; //Now on other end of map
                player = player; //Creature 0 is the player                                    
            }
            else if (c.pos.Y >= Level.GRIDH - 1)
            {
                Creature player = c.DeepClone(); //Grab copy of player
                mapPos.Y++; //We've gone to the left
                if (mapPos.X == 100)
                {
                    mapPos.X = 0; //Loop around
                }

                Random thisLevelRNG = new Random(levelSeed[mapPos.X, mapPos.Y, mapPos.Z]); //This level's generator
                if (thisLevelRNG.Next(0, 100) < 30)
                {
                    GenLevel("village", true);
                }
                else
                {
                    GenLevel("forest", true);
                }

                player.pos.Y = 2; //Now on other end of map
                player = player; //Creature 0 is the player                                    
            }
		}

        static ConsoleKey Update_GetKey()
        {
            return Console.ReadKey().Key;
        } //Hold everything and just wait for the user to press a key
        static string Update_GetString(Func<string, string>endFunc)
        {
            var input = ConsoleKey.EraseEndOfFile;
            
            string returnString = String.Empty;

            while (input != ConsoleKey.Enter &&
                input != ConsoleKey.Escape &&
                input != ConsoleKey.Backspace)
            {
                if (input == ConsoleKey.Spacebar)
                { 
                    returnString += " ";
                }
                else if(input == ConsoleKey.EraseEndOfFile)
                {
                    // do nothing.
                }
                //else if (input == "Backspace")
                //{
                //    if (returnString.Length > 1) //If there's a letter to remove
                //        returnString = returnString.Remove(input.Length - 1); //Remove last letter
                //}
                else
                {
                    returnString += CapitalizeFirst(input.ToString()); //If not a special case, add the letter
                }

                returnString += endFunc(returnString);
                
                ClearAndDraw();
                input = Update_GetKey();
            }

            return returnString;
        }
        static string Update_GetUserName()
        {
            //TESTING ONLY
            return "Will";

            //ConsoleKey input = ConsoleKey.Subtract;

            //while (input != ConsoleKey.Enter && 
            //    input != ConsoleKey.Escape)
            //{               
            //    if (input == ConsoleKey.Spacebar)
            //    {
            //        sessionName += " ";
            //    }
            //    else if (input == ConsoleKey.Backspace)
            //    {
            //        sessionName = sessionName.Substring(0, sessionName.Length - 1); //Remove last letter
            //    }
            //    else
            //    {
            //        sessionName += input; //If not a special case, add the letter
            //    }

            //    Draw();
            //    input = Update_GetKey();
            //}
            //return sessionName;
        }
        static Point Update_GetPosition()
        {
            gameState = WAIT_FOR_POSITION;
            var player = GetPlayer1();
            cursorPos = player.pos; //Start at player's position
            Point currentPos = player.pos; //The position the cursor is on
            bool done = false; //Variable for loop break

            Sdl.SDL_Event keyEvent, oldKeyEvent;
            Sdl.SDL_PollEvent(out keyEvent);            

            while (!done)
            {
                oldKeyEvent = keyEvent;
                Sdl.SDL_PollEvent(out keyEvent);
                ClearAndDraw(); //Update screen

                switch (keyEvent.type) //Test keyEvent
                {
                    case Sdl.SDL_KEYDOWN: //If a key is down
                        if (oldKeyEvent.type != Sdl.SDL_KEYDOWN ||
                            oldKeyEvent.key.keysym.sym == Sdl.SDLK_RSHIFT ||
                            oldKeyEvent.key.keysym.sym == Sdl.SDLK_LSHIFT)
                        {
                            switch (keyEvent.key.keysym.sym)
                            {
                                case Sdl.SDLK_ESCAPE:
                                    gameState = MAIN_GAME;
                                    done = true;                                    
                                    break;

                                case 271: //If enter aka "271" or "13" was pressed
                                case 13:
                                    gameState = MAIN_GAME;
                                    return cursorPos;

                                case Sdl.SDLK_KP1:
                                case Sdl.SDLK_DELETE:
                                    cursorPos.X--;
                                    cursorPos.Y++;
                                    break;

                                case Sdl.SDLK_KP2:
                                case Sdl.SDLK_DOWN:
                                    cursorPos.Y++;
                                    break;

                                case Sdl.SDLK_KP3:
                                case Sdl.SDLK_PAGEDOWN:
                                    cursorPos.X++;
                                    cursorPos.Y++;
                                    break;

                                case Sdl.SDLK_KP4:
                                case Sdl.SDLK_LEFT:
                                    cursorPos.X--;
                                    break;

                                case Sdl.SDLK_KP6:
                                case Sdl.SDLK_RIGHT:
                                    cursorPos.X++;
                                    break;

                                case Sdl.SDLK_KP7:
                                case Sdl.SDLK_INSERT:
                                    cursorPos.X--;
                                    cursorPos.Y--;
                                    break;

                                case Sdl.SDLK_KP8:
                                case Sdl.SDLK_UP:
                                    cursorPos.Y--;
                                    break;

                                case Sdl.SDLK_KP9:
                                case Sdl.SDLK_PAGEUP:
                                    cursorPos.X++;
                                    cursorPos.Y--;
                                    break;

                                default:
                                    done = false;
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            return currentPos;
        }
	}
}