using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLords_0_0_1
{
    public static class Keyboard
    {
        public static readonly Point Cancelled = new Point(Int32.MinValue, Int32.MinValue);

        public static readonly List<ConsoleKey> NumPadDirections = new List<ConsoleKey> {
                ConsoleKey.NumPad0,
                ConsoleKey.NumPad1,
                ConsoleKey.NumPad2,
                ConsoleKey.NumPad3,
                ConsoleKey.NumPad4,
                ConsoleKey.NumPad5,
                ConsoleKey.NumPad6,
                ConsoleKey.NumPad7,
                ConsoleKey.NumPad8,
                ConsoleKey.NumPad9
            };
        public static ConsoleKey ActionNumToKey(string parsedAction)
        {
            return ActionNumToKey(int.Parse(parsedAction));
        }
        public static ConsoleKey ActionNumToKey(int actionNum)
        {
            return NumPadDirections[actionNum];
        }
        public static Point DirectionNumToPoint(int direction, Point loc)
        {
            return ConsoleKeyToDirection(loc, ActionNumToKey(direction));
        }
        public static Point ConsoleKeyToDirection(Point loc, ConsoleKey key, int numSquares = 1)
        {
            var result = new Point();
            switch (key)
            {
                case ConsoleKey.NumPad1:
                    result = new Point(loc.X - numSquares, loc.Y + numSquares);
                    break;
                case ConsoleKey.NumPad2:
                    result = new Point(loc.X - 0, loc.Y + numSquares);
                    break;
                case ConsoleKey.NumPad3:
                    result = new Point(loc.X + numSquares, loc.Y + numSquares);
                    break;
                case ConsoleKey.NumPad4:
                    result = new Point(loc.X - numSquares, loc.Y - 0);
                    break;
                case ConsoleKey.NumPad5:
                    result = new Point(loc.X - 0, loc.Y - 0);
                    break;
                case ConsoleKey.NumPad6:
                    result = new Point(loc.X + numSquares, loc.Y - 0);
                    break;
                case ConsoleKey.NumPad7:
                    result = new Point(loc.X - numSquares, loc.Y - numSquares);
                    break;
                case ConsoleKey.NumPad8:
                    result = new Point(loc.X - 0, loc.Y - numSquares);
                    break;
                case ConsoleKey.NumPad9:
                    result = new Point(loc.X + numSquares, loc.Y - numSquares);
                    break;
                default:
                    return Cancelled;
            }
            return result;
        }
    }
}
