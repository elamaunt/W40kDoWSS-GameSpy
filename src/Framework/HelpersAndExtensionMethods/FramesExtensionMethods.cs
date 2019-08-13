using System.Threading.Tasks;

namespace Framework
{
    public static class FramesExtensionMethods
    {
        public static Task<Result> AttachIndicator<Result>(this Task<Result> task, IControlFrame indicator)
        {
            if (task.IsCompleted)
                return task;

            Dispatcher.RunOnMainThread(() => indicator.Visible = true);
            return task.OnContinueOnUi(() => indicator.Visible = false);
        }
    }
}
