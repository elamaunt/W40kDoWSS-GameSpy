namespace Framework
{
    public static class Service<InterfaceType>
        where InterfaceType : class
    {
        readonly static InterfaceType s_serviceInstance;
        static Service()
        {
            s_serviceInstance = Bootstrapper.GetServiceFactory<InterfaceType>()();
        }

        public static InterfaceType Get() => s_serviceInstance;
    }
}