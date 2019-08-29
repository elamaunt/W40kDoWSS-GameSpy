namespace Framework
{
    public abstract class ExtendController<ViewType, ExtensionInterfaceType, ExtensionImplementationType> : BindingController<ViewType, PageViewModel>
        where ExtensionInterfaceType : class
        where ExtensionImplementationType : ExtensionInterfaceType, IExtension, new()
        where ViewType : class
    {
        protected override bool CanBind => Frame.IsExtentionRequested<ExtensionInterfaceType>();

        protected ExtensionImplementationType Extension { get; private set; }

        protected override void OnBind()
        {
            Extend();
        }

        private void Extend()
        {
            Frame.SetExtension<ExtensionInterfaceType>(Extension = new ExtensionImplementationType());
            OnExtended();
        }

        protected abstract void OnExtended();
        
        protected override void OnUnbind()
        {
            Extension?.CleanUp();
            base.OnUnbind();
        }
    }
}
