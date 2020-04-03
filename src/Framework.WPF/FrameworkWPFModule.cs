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
            batch.RegisterControllerFactory(() => new GridWithBackgroundBinder());
            batch.RegisterControllerFactory(() => new ImageWithByteArrayFrameBinder());
            batch.RegisterControllerFactory(() => new LabelWithITextFrameBinder());
            batch.RegisterControllerFactory(() => new StackPanelWithListFrameBinder());
            batch.RegisterControllerFactory(() => new TextBlockWithITextFrameBinder());
            batch.RegisterControllerFactory(() => new ImageWithIUriFrameBinder());
            batch.RegisterControllerFactory(() => new TextBoxBaseWithITextFrameBinder());
            batch.RegisterControllerFactory(() => new TextBlockWithIActionFrameBinder());
            batch.RegisterControllerFactory(() => new ContentControlWithIListFrameBinder());
            batch.RegisterControllerFactory(() => new FrameworkElementWithIControlFrameBinder());
            batch.RegisterControllerFactory(() => new ToggleButtonWithIToggleFrameBinder());
            batch.RegisterControllerFactory(() => new FrameWithIListFrameBinder());
            batch.RegisterControllerFactory(() => new FrameWithINavigationPanelFrameBinder());
            batch.RegisterControllerFactory(() => new ListViewWithIListFrameBinder());
            batch.RegisterControllerFactory(() => new TextBoxBaseWithITextEditorFrameBinder());
            batch.RegisterControllerFactory(() => new MenuBaseWithIMenuFrameBinder());
            batch.RegisterControllerFactory(() => new MenuItemWithIMenuItemFrameBinder());
            batch.RegisterControllerFactory(() => new MenuItemWithActionFrameBinder());
            batch.RegisterControllerFactory(() => new MenuItemWithISubMenuItemFrameBinder());
            batch.RegisterControllerFactory(() => new MenuItemWithIToggleFrameBinder());
        }
    }
}
