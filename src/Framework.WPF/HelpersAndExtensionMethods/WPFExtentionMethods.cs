using System.Windows;

using Expression = System.Linq.Expressions.Expression;

namespace Framework.WPF
{
    public static class WPFExtentionMethods
    {
        public static T GetBindedFrame<T>(this FrameworkElement element)
            where T : class
        {
            return (T)FrameBinder.GetBindedFrame(element);
        }

        public static object GetBindedFrame(this FrameworkElement element)
        {
            return FrameBinder.GetBindedFrame(element);
        }

        public static T GetViewModel<T>(this FrameworkElement page)
            where T : ViewModel
        {
            return page.DataContext as T;
        }

        public static ViewModel GetViewModel(this FrameworkElement page)
        {
            return page.DataContext as ViewModel;
        }

        public static void CallInitializeComponent(this object instance)
        {
            var type = instance.GetType();
            var method = type.GetMethod("InitializeComponent");

            if (method == null)
                return;

            var instanceExpr = Expression.Variable(type);
            var callExpr = Expression.Call(instanceExpr, method);
            var lambdaEmpr = Expression.Lambda(callExpr, instanceExpr);
            lambdaEmpr.Compile().DynamicInvoke(instance);
        }
    }
}
