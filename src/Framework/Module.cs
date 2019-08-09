namespace Framework
{
    public abstract class Module
    {
        public virtual void Start()
        {
            // Nothind
        }
        public abstract void RegisterComponents(ComponentBatch batch);
    }
}