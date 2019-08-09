using System;

namespace Framework
{
    public static class Bootstrapper
    {
        static ComponentBatch s_currentBatch;
        public static ComponentBatch CurrentBatch => s_currentBatch;

        public static void Run(params Module[] modules)
        {
            if (s_currentBatch != null)
                throw new InvalidOperationException("Boostrapper already started");

            var batch = new ComponentBatch();

            for (int i = 0; i < modules.Length; i++)
            {
                var mod = modules[i];
                mod.Start();
                mod.RegisterComponents(batch);
            }

            s_currentBatch = batch;
        }
        

        internal static Func<T> GetServiceFactory<T>()
            where T : class
        {
            return s_currentBatch.GetServiceFactory<T>(); 
        }
    }
}