namespace Framework
{
    public abstract class Module
    {
        public virtual void Start()
        {
            // Nothing
        }
        public abstract void RegisterComponents(ComponentBatch batch);
    }
}