using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TimeLords_0_0_1.Creature;

namespace TimeLords_0_0_1
{
    public static class GenericExtensions
    {
        // Deep clone
        //public static T DeepClone<T>(this T a)
        //{
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        BinaryFormatter formatter = new BinaryFormatter();
        //        formatter.Serialize(stream, a);
        //        stream.Position = 0;
        //        return (T)formatter.Deserialize(stream);
        //    }
        //}

        public static Range IndexesToRange<T>(this List<T> items)
        {
            return new Range(0, items.Count);
        }
        
        public static T ChooseRandom<T>(this List<T> items) where T : new()
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            if (!items.Any())
            {
                return new T();
            }
            return items[rand.Next(0, items.Count)];
        }
    }
}
