using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThunderHawk.Utils
{
    internal class TaskHelper
    {
        public static Task<T> FromAction<T>(Action<TaskCompletionSource<T>> handler)
        {
            var tcs = new TaskCompletionSource<T>();
            handler(tcs);
            return tcs.Task;
        }

        public static async Task<T> RepeatTaskForeverIfFailed<T>(Func<Task<T>> runTask,
           int timeoutBeforeRepeat, CancellationToken token,
           string message = "Операция не удалась",
           Func<T, bool> resultHandler = null,
           Func<T, bool> repeatRefuser = null,
           Func<bool> repeatHandler = null)
        {
            var result = default(T);
            var resultReady = false;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    result = await runTask();
                    resultReady = true;
                }
                catch (Exception)
                {
                    resultReady = false;
                }

                if (resultReady)
                {
                    if (resultHandler == null)
                        return result;
                    if (resultHandler(result))
                        return result;
                    if (repeatRefuser != null && repeatRefuser(result))
                        throw new Exception(message);
                    resultReady = false;
                }

                repeatHandler?.Invoke();

                await Task.Delay(timeoutBeforeRepeat);
            }

            throw new Exception(message);
        }
    }
}