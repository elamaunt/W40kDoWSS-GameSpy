using System;

namespace Framework
{
    public class ActionFrame : ControlFrame, IActionFrame
    {
        Action _action;
        Action<object> _actionWithParameter;

        public Action Action
        {
            get => _action ?? EmptyAction;
            set
            {
                if (_action == value)
                    return;

                _action = value;
                FirePropertyChanged(nameof(Action));
            }
        }
        public Action<object> ActionWithParameter
        {
            get => _actionWithParameter ?? EmptyAction;
            set
            {
                if (_actionWithParameter == value)
                    return;

                _actionWithParameter = value;
                FirePropertyChanged(nameof(ActionWithParameter));
            }
        }

        void EmptyAction(object parameter)
        {
            // Nothing
        }

        void EmptyAction()
        {
            // Nothing
        }
    }
}
