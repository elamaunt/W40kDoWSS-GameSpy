using System;
using System.Collections.Generic;
using RandomTools.Types;

namespace RandomTools
{
    public static class Extensions
    {
        private static readonly Random Rand = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Rand.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static Dictionary<string, DowItem> CreateDict(this IEnumerable<DowItem> items)
        {
            var retDict = new Dictionary<string, DowItem>();
            foreach (var item in items)
            {
                retDict[item.Key] = item;
            }

            return retDict;
        }
    }
}
