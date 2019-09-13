using System;

namespace IrcD.Net.Tools
{
    public static class RandomHelper
    {
        static readonly Random _random = new Random();

        public static int Next(int max)
        {
            return _random.Next(max);
        }
    }
}
