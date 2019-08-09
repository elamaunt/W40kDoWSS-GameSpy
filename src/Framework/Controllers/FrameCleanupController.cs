namespace Framework
{
    internal class FrameCleanupController : FrameController<IFrame>
    {
        protected override void OnBind()
        {
            // Nothing
        }

        protected override void OnUnbind()
        {
            Frame.Cleanup();
            base.OnUnbind();
        }
    }
}
