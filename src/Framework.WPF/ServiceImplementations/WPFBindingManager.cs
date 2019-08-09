using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Framework.WPF
{
    internal class WPFBindingManager : IBindingManager
    {
        readonly ConditionalWeakTable<object, Binding> _bindingsTable = new ConditionalWeakTable<object, Binding>();

        public IBinding Bind(object view, object frame, ComponentBatch batch)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));
            if (batch == null)
                throw new ArgumentNullException(nameof(batch));

            if (_bindingsTable.TryGetValue(view, out Binding currentBinding))
                throw new InvalidOperationException($"View {view} is already binded");

            var controllers = batch.CreateControllersForTypes(view.GetType().GetTypeInfo(), frame.GetType().GetTypeInfo());
            var newBinding = new Binding(this, view, frame, controllers.Where(x => x.Bind(view, frame)).ToArray());

            _bindingsTable.Add(view, newBinding);

            return newBinding;
        }

        public IBinding GetBinding(object view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (_bindingsTable.TryGetValue(view, out Binding currentBinding))
                return currentBinding;

            return null;
        }

        void RemoveView(object view)
        {
            _bindingsTable.Remove(view);
        }

        class Binding : IBinding
        {
            bool _isUnbinded;
            IController[] _bindedControllers;
            WPFBindingManager _manager;
            public object View { get; private set; }
            public object Frame { get; private set; }
            public Binding(WPFBindingManager manager, object view, object frame, IController[] bindedControllers)
            {
                _manager = manager;
                _bindedControllers = bindedControllers;
                View = view;
                Frame = frame;
            }

            ~Binding()
            {
                try
                {
                    Dispose();
                }
                catch
                {
                }
            }

            public void Dispose()
            {
                Unbind();
            }

            public void Unbind()
            {
                if (_isUnbinded)
                    return;

                _isUnbinded = true;

                for (int i = 0; i < _bindedControllers.Length; i++)
                    _bindedControllers[i].Unbind();

                _manager.RemoveView(View);

                View = null;
                Frame = null;
            }
        }
    }
}

