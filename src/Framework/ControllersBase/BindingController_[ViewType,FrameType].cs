using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Framework
{
    public abstract class BindingController<ViewType, FrameType> : IController
        where ViewType : class
        where FrameType : class
    {
        private List<Action> _unbindActions;
        private CancellationTokenSource _usersTokenSource;

        readonly WeakReference<ViewType> _weakView = new WeakReference<ViewType>(null, false);
        readonly WeakReference<FrameType> _weakFrame = new WeakReference<FrameType>(null, false);

        protected virtual bool CanBind => true;
        protected virtual bool NeedsUnbind => true;
        
        protected bool IsBinded { get; private set; }
        protected CancellationTokenSource TokenSource => _usersTokenSource;
        public CancellationToken Token => _usersTokenSource.Token;
        public CancellationToken RecreateToken() => (_usersTokenSource = _usersTokenSource.Recreate()).Token;
        public bool Cancelled => _usersTokenSource.IsCancellationRequested;

        protected ViewType View
        {
            get
            {
                if (_weakView.TryGetTarget(out ViewType view))
                    return view;

                return null;
            }
        }

        protected FrameType Frame
        {
            get
            {
                if (_weakFrame.TryGetTarget(out FrameType frame))
                    return frame;

                return null;
            }
        }

        public bool Bind(object view, object frame)
        {
            if (!(view is ViewType typedView))
                throw new ArgumentException(nameof(view));

            if (!(frame is FrameType typedFrame))
                throw new ArgumentException(nameof(frame));

            _weakView.SetTarget(typedView);
            _weakFrame.SetTarget(typedFrame);

            if (!CanBind)
                return false;

            IsBinded = true;
            OnBind();

            return NeedsUnbind;
        }

        public void Unbind()
        {
            try
            {
                if (_unbindActions != null)
                    for (int i = 0; i < _unbindActions.Count; i++)
                        _unbindActions[i]();

                OnUnbind();
                _usersTokenSource?.CancelWithoutDisposedException();
            }
            finally
            {
                _unbindActions = null;
                IsBinded = false;
            }
        }

        protected void SubscribeOnPropertyChanged(INotifyPropertyChanged implementor, string propertyName, Action handler)
        {
            if (implementor == null)
                return;

            PropertyChangedEventHandler onPropertyChanged = (sender, args) =>
            {
                if (args.PropertyName == propertyName)
                    handler();
            };

            implementor.PropertyChanged += onPropertyChanged;
            RunWhenUnbind(() => implementor.PropertyChanged -= onPropertyChanged);
        }

        public void RunOnUIThread(Action action)
        {
            void Do()
            {
                if (IsBinded)
                    action();
            }

            Dispatcher.RunOnMainThread(Do);
        }

        protected void RunWhenUnbind(Action unbindAction)
        {
            if (_unbindActions == null)
                _unbindActions = new List<Action>();
            _unbindActions.Add(unbindAction);
        }

        protected abstract void OnBind();
        protected virtual void OnUnbind()
        {
            // Nothing
        }
    }
}
