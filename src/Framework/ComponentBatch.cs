using System;
using System.Collections.Generic;
using System.Reflection;

namespace Framework
{
    public class ComponentBatch
    {
        readonly LinkedList<ControllerRegistration> _controllerRegistrations = new LinkedList<ControllerRegistration>();
        readonly Dictionary<TypeInfo, Func<object>> _serviceRegistrations = new Dictionary<TypeInfo, Func<object>>();
        internal ComponentBatch()
        {
        }

        public IEnumerable<IController> CreateControllersForTypes(TypeInfo viewType, TypeInfo frameType)
        {
            foreach (var registration in _controllerRegistrations)
            {
                if (registration.IsAppropriateTypes(viewType, frameType))
                    yield return registration.CreateController();
            }
        }

        public void RegisterControllerFactory<ViewType, FrameType>(Func<BindingController<ViewType, FrameType>> controllerFactory)
            where ViewType : class
            where FrameType : class
        {
            _controllerRegistrations.AddLast(new ControllerRegistration(typeof(ViewType).GetTypeInfo(), typeof(FrameType).GetTypeInfo(), () => controllerFactory()));
        }

        public void RegisterServiceFactory<InterfaceType>(Func<InterfaceType> serviceFactory)
        {
            _serviceRegistrations[typeof(InterfaceType).GetTypeInfo()] = () => serviceFactory();
        }

        internal Func<InterfaceType> GetServiceFactory<InterfaceType>()
            where InterfaceType : class
        {
            if (_serviceRegistrations.TryGetValue(typeof(InterfaceType).GetTypeInfo(), out Func<object> factory))
                return () => (InterfaceType)factory();

            throw new InvalidOperationException($"There is no service for type {typeof(InterfaceType)}");
        }

        class ControllerRegistration
        {
            readonly TypeInfo _viewType;
            readonly TypeInfo _frameType;
            readonly Func<IController> _factory;

            public ControllerRegistration(TypeInfo viewType, TypeInfo frameType, Func<IController> factory)
            {
                _viewType = viewType;
                _frameType = frameType;
                _factory = factory;
            }

            public bool IsAppropriateTypes(TypeInfo viewType, TypeInfo frameType)
            {
                return _viewType.IsAssignableFrom(viewType) && _frameType.IsAssignableFrom(frameType);
            }

            public IController CreateController()
            {
                return _factory();
            }
        }
    }
}