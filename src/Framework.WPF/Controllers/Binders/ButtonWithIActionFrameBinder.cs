using System;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Framework.WPF
{
    internal class ButtonWithIActionFrameBinder : BindingController<ButtonBase, IActionFrame>
    {
        protected override void OnBind()
        {
            View.Command = new Command(Frame);
        }
        
        protected override void OnUnbind()
        {
            View.Command = null;
        }

        class Command : ICommand
        {
            readonly IActionFrame _frame;

            public Command(IActionFrame frame)
            {
                _frame = frame;
            }
            
            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
            
            public bool CanExecute(object parameter)
            {
                return _frame.Enabled;
            }

            public void Execute(object parameter)
            {
                _frame.Action?.Invoke();
            }
        }
    }
}

