using System;
using System.Windows.Input;

namespace SpectrumAnalyzer.ViewModels
{
    public class RelayCommand<T> : ICommand
    {
        /// <summary>
        /// The action to run
        /// </summary>
        private Action<T> mAction;

        private Func<bool> canExecuteEvaluator;

        /// <summary>
        /// The event that's fired when CanExecute value has changed
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<T> action, Func<bool> canExecute = null)
        {
            mAction = action;
            canExecuteEvaluator = canExecute;
        }

        /// <summary>
        /// A relay command can always execute (will cause button to be greyed out if false, etc.)
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            if (this.canExecuteEvaluator == null)
            {
                return true;
            }
            else
            {
                bool result = this.canExecuteEvaluator.Invoke();
                return result;
            }
        }

        /// <summary>
        /// Runs the action
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            mAction((T)parameter);
        }

    }
}
