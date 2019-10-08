using System;

namespace ThunderHawk.Utils
{
    public static class RandomHelper
    {
        readonly static Random _random = new Random();
        public static string GetString(int length, string chars)
        {
            return Reality.Net.Extensions.Extensions.GetString(_random, length, chars);
        }

        public static string GetString(int length)
        {
            return Reality.Net.Extensions.Extensions.GetString(_random, length);
        }
    }
}
