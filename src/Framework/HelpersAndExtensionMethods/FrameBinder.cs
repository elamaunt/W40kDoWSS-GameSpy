using System;

namespace Framework
{
    public static class FrameBinder
    {
        static IBindingManager BindingManager => Service<IBindingManager>.Get();

        public static IBinding Bind(object view, object frame)
        {
            return BindingManager.Bind(view, frame, Bootstrapper.CurrentBatch);
        }

        public static void Unbind(object view)
        {
            BindingManager.GetBinding(view)?.Unbind();
        }

        public static object GetBindedFrame(object view)
        {
            return BindingManager.GetBinding(view)?.Frame;
        }

        public static bool IsViewBinded(object view)
        {
            return BindingManager.GetBinding(view) != null;
        }
    }
}
