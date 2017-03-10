using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLords_0_0_1.Creature
{
    public enum FoodLevelText
    {
        Full,
        Overstuffed,
        Hungry,
        Famished,
        Starving,
        Organ_Failure
    }    
    public class FoodLevel
    {
        public static readonly Dictionary<FoodLevelText, FoodLevel> FoodLevels = new Dictionary<FoodLevelText, FoodLevel> {
            { FoodLevelText.Overstuffed,    new FoodLevel(new Range(15000, Int32.MaxValue), FoodLevelText.Overstuffed, Color.Cyan) },
            { FoodLevelText.Full,           new FoodLevel(new Range(10000, 15000), FoodLevelText.Full, Color.Cyan) },
            { FoodLevelText.Hungry,         new FoodLevel(new Range(5000, 10000), FoodLevelText.Hungry, Color.LightGreen) },
            { FoodLevelText.Famished,       new FoodLevel(new Range(2500, 5000), FoodLevelText.Famished, Color.Yellow) },
            { FoodLevelText.Starving,       new FoodLevel(new Range(1, 2500), FoodLevelText.Starving, Color.Crimson) },
            { FoodLevelText.Organ_Failure,  new FoodLevel(new Range(Int32.MinValue, 1), FoodLevelText.Organ_Failure, Color.Gray) },
        };
        public static readonly FoodLevel Default = FoodLevels[FoodLevelText.Full];
        public Range Range;
        public Color Color;
        public FoodLevelText Text;
        public FoodLevel(Range range, FoodLevelText text, Color color)
        {
            Range = range;
            Text = text;
            Color = color;
        }
        public override string ToString()
        {
            return Enum.GetName(typeof(FoodLevelText), this.Text);
        }
    }
}
