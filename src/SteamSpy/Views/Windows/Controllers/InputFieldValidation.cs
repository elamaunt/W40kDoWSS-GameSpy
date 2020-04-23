using System;
using System.Text.RegularExpressions;

namespace ThunderHawk
{
    public class InputFieldValidation
    {
        public static bool IsValidInputName(string name)
        {
            Regex regexObj = new Regex("(?!\\d)^[A-Za-z0-9_-]+$(?#case sensitive, matches only lower a-z)", RegexOptions.Multiline);
            Match matchResults = regexObj.Match(name);
            
            if (matchResults.Success && name.Length < 21) return true;
            return false;
        }
        
        public static bool IsValidInputPassword(string password)
        {
            Regex regexObj = new Regex("[а-яА-Я]+", RegexOptions.Multiline);
            Match matchResults = regexObj.Match(password);
            // No russian symbols in password
            if (!matchResults.Success && password.Length < 21) return true;
            return false;
        }
    }
}