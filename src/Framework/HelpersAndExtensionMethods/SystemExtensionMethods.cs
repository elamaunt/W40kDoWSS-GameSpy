using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Framework
{
    public static class SystemExtensionMethods
    {
        public static T ConvertToOrDefault<T>(this string value)
        {
            if (value.IsNullOrWhiteSpace())
                return default;

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static void Replace(this string[] array, string searchValue, string newValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (Equals(array[i], searchValue))
                    array[i] = newValue;
            }
        }

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

        public static bool IsZero(this IntPtr self)
        {
            return self == IntPtr.Zero;
        }

        public static string CutWhenEndsWith(this string self, string end)
        {
            if (self.EndsWith(end, StringComparison.OrdinalIgnoreCase))
                return self.Substring(0, self.Length - end.Length);
            return self;
        }

        public static ValueType GetOrDefault<KeyType, ValueType>(this IDictionary<KeyType, ValueType> dictionary, KeyType key)
        {
            ValueType value;
            if (dictionary.TryGetValue(key, out value))
                return value;
            return default(ValueType);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> sequence)
        {
            return new ObservableCollection<T>(sequence);
        }

        public static bool IsNullOrEmpty<ElementType>(this ICollection<ElementType> @string)
        {
            return @string == null || @string.Count == 0;
        }

        public static bool IsNullOrEmpty<ElementType>(this IList<ElementType> @string)
        {
            return @string == null || @string.Count == 0;
        }

        public static bool IsNullOrEmpty(this string @string)
        {
            return string.IsNullOrEmpty(@string);
        }

        public static bool IsNullOrWhiteSpace(this string @string)
        {
            return string.IsNullOrWhiteSpace(@string);
        }
        
        public static Exception GetLowestBaseException(this Exception exception)
        {
            Exception baseEx;
            while (exception != (baseEx = exception.GetBaseException()) || baseEx is AggregateException)
            {
                if (baseEx is AggregateException)
                {
                    exception = ((AggregateException)baseEx).InnerException;
                    continue;
                }

                exception = baseEx;
            }
            return baseEx;
        }

        public static async Task<TResult> WithBlockingCancellation<TResult>(this Task<TResult> originalTask, CancellationToken token)
        {
            var blockingTask = new TaskCompletionSource<TResult>();
            var cancelTask = new TaskCompletionSource<Void>();

            using (token.Register(t => ((TaskCompletionSource<Void>)t).TrySetResult(new Void()), cancelTask))
            {
                Task any = await Task.WhenAny(originalTask, cancelTask.Task);
                if (any != cancelTask.Task && !originalTask.IsCanceled)
                {
                    if (originalTask.IsFaulted)
                        blockingTask.SetException(originalTask.Exception);
                    else
                        blockingTask.SetResult(originalTask.Result);
                }
            }

            return await blockingTask.Task;
        }


        public static void CancelWithoutDisposedException(this CancellationTokenSource self)
        {
            try
            {
                self.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public static CancellationTokenSource Recreate(this CancellationTokenSource source)
        {
            source?.Cancel();
            return new CancellationTokenSource();
        }

        public static Task OnContinueOnUi(this Task self, Action<Task> handler)
        {
            return self.ContinueWith(t =>
            {
                Dispatcher.RunOnMainThread(() => handler(t));
                if (t.Exception != null)
                    throw t.Exception;
            });
        }

        public static Task OnContinueOnUi(this Task self, Action handler)
        {
            return self.ContinueWith(t =>
            {
                Dispatcher.RunOnMainThread(handler);
                if (t.Exception != null)
                    throw t.Exception;
            });
        }

        public static Task<T> OnFaultOnUi<T>(this Task<T> self, Action handler)
        {
            return self.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Dispatcher.RunOnMainThread(handler);

                return t.Result;
            });
        }

        public static Task OnFaultOnUi(this Task self, Action handler)
        {
            return self.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Dispatcher.RunOnMainThread(handler);

                if (t.Exception != null)
                    throw t.Exception;
            });
        }

        public static Task OnFaultOnUi(this Task self, Action<Exception> handler)
        {
            return self.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var ex = t.Exception.GetLowestBaseException();
                    Dispatcher.RunOnMainThread(() => handler(ex));
                }
                if (t.Exception != null)
                    throw t.Exception;
            });
        }

        public static Task<ResultType> OnContinueOnUi<ResultType>(this Task<ResultType> self, Action handler)
        {
            return self.ContinueWith(t =>
            {
                Dispatcher.RunOnMainThread(handler);
                return t.Result;
            });
        }

        public static Task<ResultType> OnContinueOnUi<ResultType>(this Task<ResultType> self, Action<Task<ResultType>> handler)
        {
            return self.ContinueWith(t =>
            {
                Dispatcher.RunOnMainThread(() => handler(t));
                return t.Result;
            });
        }

        public static Task<ResultType> OnFaultOnUi<ResultType>(this Task<ResultType> self, Action<Exception> handler)
        {
            return self.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var ex = t.Exception.GetLowestBaseException();
                    Dispatcher.RunOnMainThread(() => handler(ex));
                }
                return t.Result;
            });
        }

        public static Task<ResultType> OnCompletedOnUi<ResultType>(this Task<ResultType> self, Action<ResultType> handler)
        {
            return self.ContinueWith(t =>
            {
                var result = t.Result;

                if (!t.IsFaulted && !t.IsCanceled)
                    Dispatcher.RunOnMainThread(() => handler(result));

                return result;
            });
        }

        /// <summary>
        /// Empty structure for TaskCompletionSource
        /// </summary>
        public struct Void
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Заставляет компилятор убрать вызов при оптимизации
        public static void NoWarning(this Task task) { /* Не содержит кода */ }

    }
}
