using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TimeLords;
using TimeLords.Creature;

namespace TimeLords
{
    [Serializable]
    public class Sentience
    {
        int intelligence, aggression, hostility;
        bool inventoryCheck = true; //Whether we need to look at the inventory
        Random rng = new Random();
        Stack<byte> path = new Stack<byte>();
        Point targetPos = new Point(-1,-1);

        public Sentience():this(50,50,50){} //Default Constructor
        public Sentience(int intelligence, int aggression, int hostility)
        {
            this.intelligence = intelligence;
            this.aggression = aggression;
            this.hostility = hostility;
        }
        public Sentience(Sentience s)
        {
			this.rng = s.rng;
			this.path = s.path;
			this.inventoryCheck = s.inventoryCheck;
            this.intelligence = s.intelligence;
            this.aggression = s.aggression;
            this.hostility = s.hostility;
        } //Copy method

        public CreatureAction DecideAction(Level currentLevel, Creature thisCreature)
        {
            int direction = rng.Next(1, 8);
            if (direction >= 5)
                direction++;

            if (thisCreature is QuestGiver)            
                return new CreatureAction(CreatureActionType.Wait);          

            #region Array of positions
            Point[] newPos = new Point[10];
            newPos[1] = new Point(thisCreature.pos.X - 1, thisCreature.pos.Y + 1); //1
            newPos[2] = new Point(thisCreature.pos.X    , thisCreature.pos.Y + 1); //2     
            newPos[3] = new Point(thisCreature.pos.X + 1, thisCreature.pos.Y + 1); //3
            newPos[4] = new Point(thisCreature.pos.X - 1, thisCreature.pos.Y);     //4 
            newPos[6] = new Point(thisCreature.pos.X + 1, thisCreature.pos.Y);     //6
            newPos[7] = new Point(thisCreature.pos.X - 1, thisCreature.pos.Y - 1); //7 
            newPos[8] = new Point(thisCreature.pos.X    , thisCreature.pos.Y - 1); //8     
            newPos[9] = new Point(thisCreature.pos.X + 1, thisCreature.pos.Y - 1); //9 
            #endregion

            #region Gather States
            while (newPos[direction].X <= 0 || newPos[direction].X >= 80 || newPos[direction].Y <= 0 || newPos[direction].Y >= 40) //While out of bounds
            {
                direction = rng.Next(1, 8);
                if (direction >= 5) direction++;
            }

            int canAttackMeleeDir = 0;
            Point playerPos = currentLevel.creatureList[0].pos;

            for (int y = 0; y < Level.GRIDH; y++)
                for (int x = 0; x < Level.GRIDW; x++)
                {
                    if (currentLevel.tileArray[x, y].itemList.Count > 0 &&
                        currentLevel.LineOfSight(thisCreature.pos, new Point(x, y)) &&
                        path.Count == 0)
                    {
                        thisCreature.targetPos = new Point(x, y); //Item is next target if seen
                        path = currentLevel.AStarPathfind(thisCreature, thisCreature.pos, new Point(x,y));
                    }
                }
            #endregion

            #region Decide on Action
            if (currentLevel.tileArray[(int)thisCreature.pos.X,
                (int)thisCreature.pos.Y].itemList.Count > 0) //If standing over an item
            {
                foreach (BodyPart b in thisCreature.anatomy)
                {
                    if (b.canPickUpItem) //If any part can pick up items
                    {
                        inventoryCheck = true; //We're picking up an item, so we need to see what we can do with it
                        //Pick it up
                        return new CreatureAction(CreatureActionType.Pick_Up);
                    }
                }
            }

            if (currentLevel.LineOfSight(thisCreature.pos, playerPos)) //If player is newly seen or smelled
            {
                targetPos = playerPos; //Keep the last known position in creature's memory
                canAttackMeleeDir = thisCreature.AdjacentToCreatureDir(currentLevel);

                if (currentLevel.ConvertAbsolutePosToRelative(thisCreature.pos, targetPos) > 0) //If adjacent
                {
                    return new CreatureAction(CreatureActionType.Attack, currentLevel.ConvertAbsolutePosToRelative(thisCreature.pos, targetPos));
                }

                path = currentLevel.AStarPathfind(thisCreature, thisCreature.pos, playerPos); //Path to player
                return new CreatureAction(CreatureActionType.Move, path.Pop());
            }

            if (path.Count > 0) //If there's a target
            {
                if (targetPos == thisCreature.pos) //If we're standing on it
                {
                    targetPos = new Point(-1,-1); //Forget this target
                    path.Clear();
                }
                else
                {
                    if (canAttackMeleeDir == path.Peek()) //If there's a creature in our way
                    {
                        //Attack it
                        return new CreatureAction(CreatureActionType.Attack, canAttackMeleeDir);
                    }
                    if (path.Peek() > 0)
                    {
                        //Go towards target
                        return new CreatureAction(CreatureActionType.Move, path.Pop());
                    }
                    else
                    {
                        targetPos = new Point(-1, -1);
                        path.Clear();
                        //Wander
                        return new CreatureAction(CreatureActionType.Move, direction);
                    }
                }
            }

            foreach (BodyPart b in thisCreature.anatomy)
            {
                foreach (Item i in thisCreature.inventory)
                {
                    if (i is Potion && b.currentHealth < b.noInjury / 2) //If the creature has a potion and is hurt badly
                    {
                        return new CreatureAction(CreatureActionType.Eat, thisCreature.inventory.IndexOf(i));
                    }
                }
            }

            for (int y = 0; y < Level.GRIDH; y++)
                for (int x = 0; x < Level.GRIDW; x++)
                {
                    if (currentLevel.tileArray[x, y].itemList.Count > 0)
                    {
                        if (currentLevel.LineOfSight(thisCreature.pos, new Point(x, y)))
                        {
                            foreach (BodyPart b in thisCreature.anatomy)
                            {
                                if (b.canPickUpItem)
                                {
                                    targetPos = new Point(x, y);
                                    path = currentLevel.AStarPathfind(thisCreature, thisCreature.pos, playerPos); //Path to item
                                    break;
                                }
                            }
                        }
                    }
                }
            #endregion

            if (inventoryCheck)
            {
                foreach (Item i in thisCreature.inventory) //Look at all the items
                {
                    if (i is Weapon)
                    {
                        if (thisCreature.weapon == null)
                        {
                            return new CreatureAction(CreatureActionType.Wield, thisCreature.inventory.IndexOf(i));
                        }
                        else
                        {
                            if (i.damage.average > thisCreature.weapon.damage.average)
                            {
                                return new CreatureAction(CreatureActionType.Unwield);
                            }
                        }
                    }

                    if (i is Armor)
                    {
                        if (thisCreature.CanWear((Armor)i))
                        {
                            return new CreatureAction(CreatureActionType.Wear, thisCreature.inventory.IndexOf(i));
                        }
                    }
                }

                inventoryCheck = false; //If we've checked everything and can't find a use, don't bother for a while
            }

            var newPoint = Keyboard.DirectionNumToPoint(direction, thisCreature.pos);
            while (!thisCreature.CanMoveBorder(newPoint))
            {
                direction = rng.Next(1, 9);
                if (direction >= 5)
                    direction++;
            }
            return new CreatureAction(CreatureActionType.Move, new List<int> { newPoint.X, newPoint.Y });
        }
        public bool ShouldBeHostileTo(int creatureNumber)
        {
            if (hostility >= 100) //If completely bloodthirsty
            {
                return true; //Always be hostile
            }

            if (creatureNumber == 0) //If the player
            {
                return true; //Always player-hostile for now.
            }

            return false; //If nothing else caught it, assume false
        }
    } //A mind, in which is held Artificial Intelligence. Or a long series of 'if' statements
}
