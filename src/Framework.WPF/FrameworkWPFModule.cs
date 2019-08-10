namespace Framework.WPF
{
    public class FrameworkWPFModule : Module
    {
        public override void RegisterComponents(ComponentBatch batch)
        {
            batch.RegisterServiceFactory<IBindingManager>(() => new WPFBindingManager());
            batch.RegisterServiceFactory<IMainThreadDispatcher>(() => new WPFMainThreadDispatcher());
            batch.RegisterServiceFactory<IViewFactory>(() => new WPFViewFactory());
            
            batch.RegisterControllerFactory(() => new ButtonWithIActionFrameBinder());
            batch.RegisterControllerFactory(() => new ButtonWithITextFrameBinder());
            batch.RegisterControllerFactory(() => new ImageWithByteArrayFrameBinder());
            batch.RegisterControllerFactory(() => new LabelWithITextFrameBinder());
            batch.RegisterControllerFactory(() => new StackPanelWithListFrameBinder());
            batch.RegisterControllerFactory(() => new TextBlockWithITextFrameBinder());
            batch.RegisterControllerFactory(() => new ImageWithIUriFrameBinder());
        }
    }
}
