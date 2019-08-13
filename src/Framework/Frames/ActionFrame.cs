using System;

namespace Framework
{
    public class ActionFrame : ControlFrame, IActionFrame
    {
        Action _action;

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

        void EmptyAction()
        {
            // Nothing
        }
    }
}
