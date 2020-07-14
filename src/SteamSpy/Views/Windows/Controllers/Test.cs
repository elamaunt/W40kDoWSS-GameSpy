using System;
using System.Text.RegularExpressions;

namespace ThunderHawk
{
    public static class Test
    {
        public static void Run()
        {
            Regex regexObj = new Regex("[а-яА-Я]+", RegexOptions.Multiline);
            Match matchResults = regexObj.Match("asdsad");
            if (matchResults.Success)
            {
                Console.Out.Write("нахуй");
            }
        }
    }
}