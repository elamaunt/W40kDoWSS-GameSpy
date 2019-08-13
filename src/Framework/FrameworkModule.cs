namespace Framework
{
    public class FrameworkModule : Module
    {
        public override void RegisterComponents(ComponentBatch batch)
        {
            batch.RegisterControllerFactory(() => new FrameCleanupController());
            batch.RegisterServiceFactory<ILogService>(() => new NLoggerLogService());
        }
    }
}
