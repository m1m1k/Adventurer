using System.Collections.Generic;
using System.Drawing;
using TimeLords;
using TimeLords_0_0_1.Creature;

namespace TimeLords_0_0_1.General
{
    public static class Mathematics
    {
        public const double PI = 3.14159265; //Circumference/Diameter

        public static bool IsWithinRange(int target, Range range)
        {
            return range.Min <= target && target <= range.Max;
        }
        public static bool IsWithinBounds(Point point, Tile[,] tiles)
        {
            return IsWithinRange(point.X, new Range(0, tiles.GetLength(0))) &&
                IsWithinRange(point.Y, new Range(0, tiles.GetLength(1)));
        }
        public static bool IsWithinBounds(Point point, Range xRange, Range yRange)
        {
            return IsWithinRange(point.X, xRange) &&
                IsWithinRange(point.Y, yRange);
        }
        public static int ForceIntoArrayBounds(int inputIndex, Range bounds)
        {
            if (inputIndex >= bounds.Max)
            {
                inputIndex = (int)bounds.Max;
            }
            if (inputIndex < bounds.Min)
            {
                inputIndex = (int)bounds.Min;
            }
            return inputIndex;
        }
        public static int ForceIntoArrayBounds<T>(int inputIndex, List<T> items)
        {
            return ForceIntoArrayBounds(inputIndex, items.IndexesToRange());
        }
    }
}
