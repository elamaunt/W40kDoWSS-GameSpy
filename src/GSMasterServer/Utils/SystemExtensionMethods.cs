using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace System
{
    public static class SystemExtensinoMethods
    {
        public static T OfJson<T>(this string value)
        {
            if (value.IsNullOrEmpty())
                return default;

            return JsonConvert.DeserializeObject<T>(value);
        }

        public static string AsJson(this object value)
        {
            if (value == null)
                return null;

            return JsonConvert.SerializeObject(value);
        }

        public static bool IsNullOrEmpty(this string self)
        {
            return string.IsNullOrEmpty(self);
        }

        public static bool IsNullOrEmpty(this IList self)
        {
            return self == null || self.Count == 0;
        }


        public static bool IsNullOrWhiteSpace(this string self)
        {
            return string.IsNullOrWhiteSpace(self);
        }

        public static T GetOrDefault<B, T>(this IDictionary<B, T> self, B key)
        {
            if (self.TryGetValue(key, out T value))
                return value;

            return default(T);
        }
    }
}
