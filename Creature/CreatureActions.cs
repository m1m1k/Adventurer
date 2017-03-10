using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLords;

namespace TimeLords_0_0_1.Creature
{
    public class Range
    {
        public double Max;
        public double Min;
        public Range(double min, double max)
        {
            Min = min;
            Max = max;
        }
    }
    public class CreatureAction
    {
        public const int UnknownInput = Int32.MinValue;
        public CreatureActionType Type;
        public List<int> Inputs;
        public CreatureAction(CreatureActionType action) : this(action, new List<int>())
        { }
        public CreatureAction(CreatureActionType action, int input) : this(action, new List<int> { input })
        { }
        public CreatureAction(CreatureActionType action, List<int> input)
        {
            Type = action;
            Inputs = input;
        }
    }
    public enum CreatureActionType
    {
        Pick_Up,
        Unwield,
        Remove,
        Wait,
        Wield,
        Wear,
        Eat,
        Attack,
        Move
    }
}
