using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace System
{
    public static class SystemExtensionMethods
    {
        public static float Clamp(this float self, float min = 0f, float max = 1f)
        {
            if (self > max)
                return max;

            if (self < min)
                return min;

            return self;
        }

        public static double Clamp(this double self, double min = 0.0, double max = 1.0)
        {
            if (self > max)
                return max;

            if (self < min)
                return min;

            return self;
        }

        public static decimal Clamp(this decimal self, decimal min = 0m, decimal max = 1m)
        {
            if (self > max)
                return max;

            if (self < min)
                return min;

            return self;
        }

        public static void CancelSafety(this CancellationTokenSource self)
        {
            try
            {
                self.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public static void Clear<T>(this ConcurrentQueue<T> self)
        {
            T t;
            while (self.Any())
                self.TryDequeue(out t);
        }
        public static void EnqueueRange<T>(this ConcurrentQueue<T> self, IEnumerable<T> range)
        {
            foreach (var item in range)
                self.Enqueue(item);
        }

        public static string ToFloatString(this float self)
        {
            return self.ToString(CultureInfo.InvariantCulture).Replace(',', '.') + "f";
        }

        public static object GetDefaultValue(this Type self)
        {
            if (self.IsValueType)
                return Activator.CreateInstance(self);

            return null;
        }

        public static string ToString(this char self, int count)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < count; i++)
            {
                builder.Append(self);
            }

            return builder.ToString();
        }

        public static int ToInt(this bool obj)
        {
            return obj ? 1 : 0;
        }
        
        public static bool IsNull(this object self)
        {
            return self == null;
        }

        public static bool NotNull(this object self)
        {
            return self != null;
        }

        public static void PushRange<T>(this Stack<T> self, IEnumerable<T> range)
        {
            foreach (var item in range)
                self.Push(item);
        }

        public static void AppendLines(this StringBuilder self, IEnumerable<string> lines)
        {
            foreach (var item in lines)
                self.AppendLine(item);
        }

        public static bool IsZero(this IntPtr self)
        {
            return self == IntPtr.Zero;
        }

        public static bool HasSameType(this object self, object other)
        {
            return self.GetType() == other.GetType();
        }

        public static string ToBase64(this string self)
        {
            var textBytes = Encoding.UTF8.GetBytes(self);
            return Convert.ToBase64String(textBytes);
        }

        public static string ToBase64(this int self)
        {
            var textBytes = Encoding.UTF8.GetBytes(self.ToString());
            return Convert.ToBase64String(textBytes);
        }

        public static string ToBase64(this ulong self)
        {
            var textBytes = Encoding.UTF8.GetBytes(self.ToString());
            return Convert.ToBase64String(textBytes);
        }

        public static int ToIntSafety(this ulong self)
        {
            if (self > int.MaxValue)
                return int.MaxValue;

            return Convert.ToInt32(self);
        }

        public static ulong ToUlong(this int self)
        {
            if (self < 0)
                return 0ul;

            return Convert.ToUInt64(self);
        }

        public static int ToIntSafety(this double self)
        {
            if (self > int.MaxValue)
                return int.MaxValue;

            return Convert.ToInt32(self);
        }

        public static bool IsNullOrEmpty(this string self)
        {
            return string.IsNullOrEmpty(self);
        }

        public static bool IsNullOrWhiteSpace(this string self)
        {
            return string.IsNullOrWhiteSpace(self);
        }

        public static float ParseToFloatOrDefault(this string self, float defaultValue = 0)
        {
            if (!self.IsNullOrWhiteSpace())
            {
                float value;
                if (float.TryParse(self, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                    return value;
            }
            return defaultValue;
        }

        public static bool ParseToBoolOrDefault(this string self, bool defaultValue = false)
        {
            if (!self.IsNullOrWhiteSpace())
            {
                bool value;
                if (bool.TryParse(self, out value))
                    return value;
            }
            return defaultValue;
        }

        public static string ToIntString(this object self)
        {
            return ((int)self).ToString();
        }

        public static T ToEnumFromIntString<T>(this string self)
            where T : struct
        {
            if (!self.IsNullOrWhiteSpace())
            {
                int value;
                if (int.TryParse(self, out value))
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }

            return default(T);
        }

        public static T ParseToEnumOrDefault<T>(this string self, T defaultValue = default(T))
            where T : struct
        {
            if (!self.IsNullOrWhiteSpace())
            {
                T value;
                if (Enum.TryParse<T>(self, out value))
                    return value;
            }

            return defaultValue;
        }

        public static string TrimSpacesAndTabs(this string self)
        {
            return self.Trim(' ', '\u0009');
        }

        public static Exception GetInnerException(this Exception self)
        {
            while (self.InnerException != null)
                self = self.InnerException;

            return self;
        }
    }
}
