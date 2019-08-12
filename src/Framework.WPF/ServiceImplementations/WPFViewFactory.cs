namespace Framework.WPF
{
    public class WPFViewFactory : IViewFactory
    {
        public object CreateView(string prefix, string name)
        {
            switch (prefix)
            {
                case "element": return WPFPageHelper.InstantiateControl(name);
                case "window": return WPFPageHelper.InstantiateWindow(name);
                case "page": return WPFPageHelper.InstantiatePage(name);
                default: return WPFPageHelper.InstantiateControl(prefix, name);
            }
        }
    }
}
