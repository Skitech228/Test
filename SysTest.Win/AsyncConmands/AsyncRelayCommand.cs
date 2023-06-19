#region Using derectives

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using SysTest.Win.Extentions;

#endregion

namespace SysTest.Win.AsyncConmands
{
    public class AsyncRelayCommand : IAsyncCommand
    {
        private readonly Func<bool> _canExecute;
        private readonly IErrorHandler _errorHandler;
        private readonly Func<Task> _execute;

        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute,
                                 Func<bool> canExecute = null,
                                 IErrorHandler errorHandler = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (() => true);
            _errorHandler = errorHandler;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute() => !_isExecuting && (_canExecute?.Invoke() ?? true);

        public async Task ExecuteAsync()
        {
            if (CanExecute())
            {
                try
                {
                    _isExecuting = true;
                    await _execute();
                }
                finally
                {
                    _isExecuting = false;
                }
            }

            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        #region Explicit implementations

        bool ICommand.CanExecute(object parameter) => CanExecute();

        void ICommand.Execute(object parameter) => ExecuteAsync()
                .FireAndForgetSafeAsync(_errorHandler);

        #endregion
    }
}