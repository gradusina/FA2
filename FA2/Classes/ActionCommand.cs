using System;
using System.Windows.Input;

namespace FA2.Classes
{
    public class ActionCommand : ICommand
    {
        private readonly Action _action;
        private readonly Action<object> _objectAction;
        private bool _isExecutable;

        public bool IsExecutable
        {
            get { return _isExecutable; }
            set
            {
                _isExecutable = value;
                if (CanExecuteChanged == null)
                {
                    return;
                }

                CanExecuteChanged(this, new EventArgs());
            }
        }

        public ActionCommand(Action action)
        {
            _action = action;
        }

        public ActionCommand(Action<object> action)
        {
            _objectAction = action;
        }

        /// <summary>
        /// Предикат показывает можно ли запускать команды при заданном аргументе.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return IsExecutable;
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Что должна выполнять команда
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            if (_action != null)
                _action();
            else if (_objectAction != null)
            {
                _objectAction(parameter);
            }
        }
    }
}
